using System;
using System.IO;
using System.Text;

namespace ZxSharpService.Game
{
    public static class BinaryExtensions
    {
        public static void WriteUnicode(this BinaryWriter writer, string text, int len)
        {
            var unicode = Encoding.Unicode.GetBytes(text);
            var result = new byte[len * 2];
            var max = len * 2 - 2;
            Array.Copy(unicode, result, unicode.Length > max ? max : unicode.Length);
            writer.Write(result);
        }

        public static string ReadUnicode(this BinaryReader reader, int len)
        {
            var unicode = reader.ReadBytes(len * 2);
            var text = Encoding.Unicode.GetString(unicode);
            text = text.Substring(0, text.IndexOf('\0'));
            return text;
        }
    }
}