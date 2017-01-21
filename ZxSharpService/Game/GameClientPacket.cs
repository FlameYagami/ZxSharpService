using System.IO;
using ZxSharpService.Game.Enums;

namespace ZxSharpService.Game
{
    public class GameClientPacket
    {
        private readonly BinaryReader _mReader;

        public GameClientPacket(byte[] content)
        {
            Content = content;
            _mReader = new BinaryReader(new MemoryStream(Content));
        }

        public byte[] Content { get; }

        public CtosMessage ReadCtos()
        {
            return (CtosMessage) _mReader.ReadByte();
        }

        public byte ReadByte()
        {
            return _mReader.ReadByte();
        }

        public byte[] ReadToEnd()
        {
            return _mReader.ReadBytes((int) _mReader.BaseStream.Length - (int) _mReader.BaseStream.Position);
        }

        public sbyte ReadSByte()
        {
            return _mReader.ReadSByte();
        }

        public short ReadInt16()
        {
            return _mReader.ReadInt16();
        }

        public int ReadInt32()
        {
            return _mReader.ReadInt32();
        }

        public uint ReadUInt32()
        {
            return _mReader.ReadUInt32();
        }

        public string ReadUnicode(int len)
        {
            return _mReader.ReadUnicode(len);
        }

        public long GetPosition()
        {
            return _mReader.BaseStream.Position;
        }

        public void SetPosition(long pos)
        {
            _mReader.BaseStream.Position = pos;
        }
    }
}