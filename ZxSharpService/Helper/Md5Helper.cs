using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ZxSharpService.Helper
{

    public class Md5Helper
    {
        public static byte[] RemoveMd5(byte[] bytes)
        {
            var checkBytes = new byte[bytes.Length - 2];
            Array.Copy(bytes, checkBytes,bytes.Length - 2);
            return checkBytes;
        }

        /// <summary>
        /// 获取一个String的Md5值
        /// </summary>
        /// <returns>返回计算结果的前4位</returns>
        public static byte[] Calculate(byte[] bytes)
        {
            var temp = new MD5CryptoServiceProvider().ComputeHash(bytes);
            var md5Bytes = new byte[2];
            Array.Copy(temp, md5Bytes, 2);
            return md5Bytes;
        }

        public static bool Check(byte[] bytes)
        {
            var checkBytes = new byte[bytes.Length - 2];
            var md5Bytes = new byte[2];
            Array.ConstrainedCopy(bytes, 0, checkBytes, 0, bytes.Length - 2);
            Array.ConstrainedCopy(bytes, bytes.Length - 2, md5Bytes, 0, 2);
            return  BitConverter.ToString(Calculate(checkBytes)).Equals(BitConverter.ToString(md5Bytes));
        }
    }
}
