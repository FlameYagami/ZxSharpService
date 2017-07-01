using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using ZxSharpService.Helper;

namespace ZxSharpService
{
    internal class Program
    {
        private const string Version = "0.2 Beta";
        public static Random Random;

        public static ServerConfig Config { get; private set; }

        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Config = new ServerConfig();
            var loaded = args.Length > 1 ? Config.Load(args[1]) : Config.Load();

            CardManager.Init();

            Logger.WriteLine(loaded ? "Config loaded." : "Unable to load config.txt, using default settings.");

            Logger.WriteLine("Accepting client version 0x" + Config.ClientVersion.ToString("x") + " or better.");

            var coreport = 0;

            if (args.Length > 0)
                int.TryParse(args[0], out coreport);

            Random = new Random();

            var server = new Server();
            if (!server.Start(coreport))
                Thread.Sleep(5000);

            while (server.IsListening)
            {
                server.Process();
                Thread.Sleep(1);
            }
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception ?? new Exception();

            File.WriteAllText("crash_" + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", exception.ToString());

            Process.GetCurrentProcess().Kill();
        }
    }
}