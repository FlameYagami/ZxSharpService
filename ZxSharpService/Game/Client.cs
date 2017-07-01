using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using ZxSharpService.Helper;

namespace ZxSharpService.Game
{
    public class Client
    {
        private readonly TcpClient _mClient;
        private readonly BinaryReader _mReader;
        private readonly Queue<ClientPacket> _mRecvQueue;
        private readonly Queue<byte[]> _mSendQueue;
        private bool _mClosePending;
        private bool _mDisconnected;
        private int _mReceivedLen;
        private GameRoom _mRoom;

        public Client(TcpClient client)
        {
            IsConnected = true;
            Player = new Player(this);
            _mClient = client;
            _mReader = new BinaryReader(_mClient.GetStream());
            _mRecvQueue = new Queue<ClientPacket>();
            _mSendQueue = new Queue<byte[]>();
            _mReceivedLen = -1;
        }

        public bool IsConnected { get; private set; }
        public Game Game { get; private set; }
        public Player Player { get; }

        public void Close()
        {
            if (!IsConnected)
                return;
            IsConnected = false;
            _mClient.Close();
            if (InGame())
                _mRoom.RemoveClient(this);
        }

        public bool InGame()
        {
            return Game != null;
        }

        public void JoinGame(GameRoom room)
        {
            if (_mRoom != null) return;
            _mRoom = room;
            Game = _mRoom.Game;
        }

        public void CloseDelayed()
        {
            _mClosePending = true;
        }

        public void Send(byte[] raw)
        {
            _mSendQueue.Enqueue(raw);
        }

        public void Tick()
        {
            if (IsConnected)
                try
                {
                    CheckDisconnected();
                    NetworkSend();
                    NetworkReceive();
                }
                catch (Exception)
                {
                    _mDisconnected = true;
                }
            if (_mClosePending)
            {
                _mDisconnected = true;
                Close();
                return;
            }
            if (!_mDisconnected)
                try
                {
                    NetworkParse();
                }
                catch (Exception ex)
                {
                    Logger.WriteError(ex);
                    _mDisconnected = true;
                }
            if (!_mDisconnected) return;
            Close();
            Player.OnDisconnected();
        }

        /// <summary>
        /// 检查连接状态
        /// </summary>
        private void CheckDisconnected()
        {
            _mDisconnected = _mClient.Client.Poll(1, SelectMode.SelectRead) && _mClient.Available == 0;
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        private void NetworkReceive()
        {
            if (_mClient.Available >= 2 && _mReceivedLen == -1) _mReceivedLen = _mClient.Available;
            if (_mReceivedLen == -1 || _mClient.Available < _mReceivedLen) return;
            var bytes = _mReader.ReadBytes(_mReceivedLen);
            _mReceivedLen = -1;

            // Md5校验
            if (!Md5Helper.Check(bytes))
            {
                Logger.WriteError("Md5校验失败->" + Convert.ToString(bytes));
                return;
            }
            Logger.WriteBytes(bytes);
            // 去除Md5校验码
            var removeMd5 = Md5Helper.RemoveMd5(bytes);
            // 添加信息到队列当中
            var packet = new ClientPacket(removeMd5);
            lock (_mRecvQueue)
            {
                _mRecvQueue.Enqueue(packet);
            }
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        private void NetworkSend()
        {
            while (_mSendQueue.Count > 0)
            {
                var calculateBytes = _mSendQueue.Dequeue();
                var stream = new MemoryStream();
                var writer = new BinaryWriter(stream);
                writer.Write(calculateBytes.Length + 2);
                writer.Write(calculateBytes);
                writer.Write(Md5Helper.Calculate(calculateBytes));
                _mClient.Client.Send(stream.ToArray());
            }
        }

        private void NetworkParse()
        {
            int count;
            lock (_mRecvQueue)
            {
                count = _mRecvQueue.Count;
            }
            while (count > 0)
            {
                ClientPacket packet = null;
                lock (_mRecvQueue)
                {
                    if (_mRecvQueue.Count > 0)
                        packet = _mRecvQueue.Dequeue();
                    count = _mRecvQueue.Count;
                }
                if (packet != null)
                    Player.Parse(packet);
            }
        }
    }
}