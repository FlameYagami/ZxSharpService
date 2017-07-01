using System;
using System.Data;
using System.IO;
using System.Text;
using ZxSharpService.Game.Enums;
using ZxSharpService.Helper;

namespace ZxSharpService.Game
{
    public class ClientPacket
    {
        private readonly BinaryReader _mReader;

        public ClientPacket(byte[] bytes)
        {
            _mReader = new BinaryReader(new MemoryStream(bytes));
        }

        /// <summary>
        /// 解析设备类型
        /// </summary>
        /// <returns></returns>
        public DevTypeMessage ReadDevType()
        {
            return (DevTypeMessage)_mReader.ReadByte();
        }

        /// <summary>
        /// 解析指令
        /// </summary>
        /// <returns></returns>
        public ClientMessage ReadCmd()
        {
            return (ClientMessage)_mReader.ReadByte();
        }

        public sbyte ReadSBytes()
        {
            return _mReader.ReadSByte();
        }

        public byte ReadByte()
        {
            return _mReader.ReadByte();
        }

        public string ReadStringToEnd()
        {
            return Encoding.UTF8.GetString(ReadBytesToEnd());
        }

        public byte[] ReadBytesToEnd()
        {
            return _mReader.ReadBytes((int)_mReader.BaseStream.Length - (int)_mReader.BaseStream.Position);
        }
    }
}