using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace ZxSharpService.Game
{
    public class GameClient
    {
        private readonly TcpClient _mClient;
        private bool _mClosePending;
        private bool _mDisconnected;
        private readonly BinaryReader _mReader;
        private int _mReceivedLen;
        private readonly Queue<GameClientPacket> _mRecvQueue;
        private GameRoom _mRoom;
        private readonly Queue<byte[]> _mSendQueue;

        public GameClient(TcpClient client)
        {
            IsConnected = true;
            Player = new Player(this);
            _mClient = client;
            _mReader = new BinaryReader(_mClient.GetStream());
            _mRecvQueue = new Queue<GameClientPacket>();
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

        private void CheckDisconnected()
        {
            _mDisconnected = _mClient.Client.Poll(1, SelectMode.SelectRead) && _mClient.Available == 0;
        }

        private void NetworkReceive()
        {
            if (_mClient.Available >= 2 && _mReceivedLen == -1)
                _mReceivedLen = _mReader.ReadUInt16();

            if (_mReceivedLen == -1 || _mClient.Available < _mReceivedLen) return;
            var packet = new GameClientPacket(_mReader.ReadBytes(_mReceivedLen));
            _mReceivedLen = -1;
            lock (_mRecvQueue)
            {
                _mRecvQueue.Enqueue(packet);
            }
        }

        private void NetworkSend()
        {
            while (_mSendQueue.Count > 0)
            {
                var raw = _mSendQueue.Dequeue();
                var stream = new MemoryStream(raw.Length + 2);
                var writer = new BinaryWriter(stream);
                writer.Write((ushort) raw.Length);
                writer.Write(raw);
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
                GameClientPacket packet = null;
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