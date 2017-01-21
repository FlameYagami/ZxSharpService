using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using ZxWrapper.Enums;
using ZxWrapper.Helpers;

namespace ZxSharpService.Game
{
    public class Duel
    {
        internal static IDictionary<IntPtr, Duel> Duels;
        private Func<GameMessage, BinaryReader, byte[], int> _mAnalyzer;
        private readonly IntPtr _mBuffer;
        private Action<string> _mErrorHandler;
        private readonly IntPtr _mPDuel;

        internal Duel(IntPtr pDuel)
        {
            _mBuffer = Marshal.AllocHGlobal(4096);
            _mPDuel = pDuel;
            Duels.Add(_mPDuel, this);
        }

        public void SetAnalyzer(Func<GameMessage, BinaryReader, byte[], int> analyzer)
        {
            _mAnalyzer = analyzer;
        }

        public void SetErrorHandler(Action<string> errorHandler)
        {
            _mErrorHandler = errorHandler;
        }

        public void InitPlayers(int startLp, int startHand, int drawCount)
        {
            //Api.set_player_info(_mPDuel, 0, startLp, startHand, drawCount);
            //Api.set_player_info(_mPDuel, 1, startLp, startHand, drawCount);
        }

        public void AddCard(int cardId, int owner, CardLocation location)
        {
            //Api.new_card(_mPDuel, (uint) cardId, (byte) owner, (byte) owner, (byte) location, 0, 0);
        }

        public void AddTagCard(int cardId, int owner, CardLocation location)
        {
            //Api.new_tag_card(_mPDuel, (uint) cardId, (byte) owner, (byte) location);
        }

        public void Start(int options)
        {
            //Api.start_duel(_mPDuel, options);
        }

        public int Process()
        {
            return -1;
            //var fail = 0;
            //while (true)
            //{
            //    int result = Api.process(_mPDuel);
            //    var len = result & 0xFFFF;

            //    if (len > 0)
            //    {
            //        fail = 0;
            //        var arr = new byte[4096];
            //        Api.get_message(_mPDuel, _mBuffer);
            //        Marshal.Copy(_mBuffer, arr, 0, 4096);
            //        result = HandleMessage(new BinaryReader(new MemoryStream(arr)), arr, len);
            //        if (result != 0)
            //            return result;
            //    }
            //    else if (++fail == 10)
            //    {
            //        return -1;
            //    }
            //}
        }

        public void SetResponse(int resp)
        {
            //Api.set_responsei(_mPDuel, (uint) resp);
        }

        public void SetResponse(byte[] resp)
        {
            //if (resp.Length > 64) return;
            //var buf = Marshal.AllocHGlobal(64);
            //Marshal.Copy(resp, 0, buf, resp.Length);
            //Api.set_responseb(_mPDuel, buf);
            //Marshal.FreeHGlobal(buf);
        }

        public int QueryFieldCount(int player, CardLocation location)
        {
            return -1;
            //return Api.query_field_count(_mPDuel, (byte) player, (byte) location);
        }

        public byte[] QueryFieldCard(int player, CardLocation location, int flag, bool useCache)
        {
            //int len = Api.query_field_card(_mPDuel, (byte) player, (byte) location, flag, _mBuffer, useCache ? 1 : 0);
            //var result = new byte[len];
            //Marshal.Copy(_mBuffer, result, 0, len);
            return null;
        }

        public byte[] QueryCard(int player, int location, int sequence, int flag)
        {
            //int len = Api.query_card(_mPDuel, (byte) player, (byte) location, (byte) sequence, flag, _mBuffer, 0);
            //var result = new byte[len];
            //Marshal.Copy(_mBuffer, result, 0, len);
            return null;
        }

        public void End()
        {
            //Api.end_duel(_mPDuel);
            //Dispose();
        }

        public IntPtr GetNativePtr()
        {
            return _mPDuel;
        }

        internal void Dispose()
        {
            Marshal.FreeHGlobal(_mBuffer);
            Duels.Remove(_mPDuel);
        }

        internal void OnMessage(uint messageType)
        {
            //var arr = new byte[256];
            //Api.get_log_message(_mPDuel, _mBuffer);
            //Marshal.Copy(_mBuffer, arr, 0, 256);
            //var message = Encoding.UTF8.GetString(arr);
            //if (message.Contains("\0"))
            //    message = message.Substring(0, message.IndexOf('\0'));
            //if (_mErrorHandler != null)
            //    _mErrorHandler.Invoke(message);
        }

        private int HandleMessage(BinaryReader reader, byte[] raw, int len)
        {
            while (reader.BaseStream.Position < len)
            {
                var msg = (GameMessage) reader.ReadByte();
                var result = -1;
                if (_mAnalyzer != null)
                    result = _mAnalyzer.Invoke(msg, reader, raw);
                if (result != 0)
                    return result;
            }
            return 0;
        }

        public static Duel Create(uint seed)
        {
            var random = new MtRandom();
            random.Reset(seed);
            //IntPtr pDuel = Api.create_duel(random.Rand());
            //return Create(pDuel);
            return null;
        }

        internal static Duel Create(IntPtr pDuel)
        {
            return pDuel == IntPtr.Zero ? null : new Duel(pDuel);
        }
    }
}