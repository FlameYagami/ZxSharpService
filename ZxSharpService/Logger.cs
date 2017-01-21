using System;
using System.IO;

namespace ZxSharpService
{
    internal class Logger
    {
        public static void WriteLine(object text, bool useTag = true)
        {
            if (Program.Config.ConsoleLog)
                Console.WriteLine((useTag ? "[Log] " : "") + text);
        }

        public static void WriteError(object error)
        {
            if (Program.Config.ConsoleLog)
                Console.WriteLine("[Error] " + error);
            WriteError("[Error] " + error);
        }

        public static void WriteLine(object type, object text)
        {
            if (Program.Config.ConsoleLog)
                Console.WriteLine("[" + type + "] " + text);
        }

        private static void WriteError(string text)
        {
            if (Program.Config.Log)
                try
                {
                    var writer = new StreamWriter("ErrorLog.txt", true);
                    writer.WriteLine(text);
                    writer.Close();
                }
                catch (Exception ex)
                {
                    if (Program.Config.ConsoleLog)
                        Console.WriteLine(ex);
                }
        }
    }
}