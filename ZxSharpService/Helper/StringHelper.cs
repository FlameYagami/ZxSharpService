using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxSharpService.Helper
{
    public class StringHelper
    {
        public static byte Hex2Byte(string src)
        {
            var hexChars = src.ToCharArray();
            return (byte) (Char2Byte(hexChars[0]) << 4 | Char2Byte(hexChars[1]));
        }

        public static byte[] Hex2Bytes(string src)
        {
            int length = src.Length / 2;
            char[] hexChars = src.ToCharArray();
            byte[] d = new byte[length];
            for (int i = 0; i < length; i++)
            {
                int pos = i * 2;
                d[i] = (byte)(Char2Byte(hexChars[pos]) << 4 | Char2Byte(hexChars[pos + 1]));
            }
            return d;
        }

        private static byte Char2Byte(char c)
        {
            return (byte)"0123456789ABCDEF".IndexOf(c);
        }
    }
}
