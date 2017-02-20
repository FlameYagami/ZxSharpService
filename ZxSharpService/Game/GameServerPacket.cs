using System.IO;
using ZxSharpService.Game.Enums;
using ZxWrapper.Enums;

namespace ZxSharpService.Game
{
    public class GameServerPacket
    {
        private readonly MemoryStream _mStream;
        private readonly BinaryWriter _mWriter;

        public GameServerPacket(StocMessage message)
        {
            _mStream = new MemoryStream();
            _mWriter = new BinaryWriter(_mStream);
            Write((byte)4);
            Write((byte)message);
        }

        public GameServerPacket(GameMessage message)
        {
            _mStream = new MemoryStream();
            _mWriter = new BinaryWriter(_mStream);
            _mWriter.Write((byte) StocMessage.GameMsg);
            _mWriter.Write((byte) message);
        }

        public byte[] GetBytes()
        {
            return _mStream.ToArray();
        }

        public void Write(byte[] array)
        {
            _mWriter.Write(array);
        }

        public void Write(bool value)
        {
            _mWriter.Write((byte) (value ? 1 : 0));
        }

        public void Write(sbyte value)
        {
            _mWriter.Write(value);
        }

        public void Write(string str)
        {
            _mWriter.Write(str);
        }

        public void Write(byte value)
        {
            _mWriter.Write(value);
        }

        public void Write(short value)
        {
            _mWriter.Write(value);
        }

        public void Write(int value)
        {
            _mWriter.Write(value);
        }

        public void Write(uint value)
        {
            _mWriter.Write(value);
        }

        public void Write(string text, int len)
        {
            _mWriter.WriteUnicode(text, len);
        }

        public long GetPosition()
        {
            return _mStream.Position;
        }

        public void SetPosition(long pos)
        {
            _mStream.Position = pos;
        }
    }
}