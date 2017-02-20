﻿using System;
using System.Diagnostics;
using System.IO;

namespace ZxSharpService
{
    internal class ServerConfig
    {
        public ServerConfig()
        {
            ClientVersion = 0x1;
            ServerPort = 8989;
            Path = ".";
            ScriptFolder = "script";
            BanlistFile = "lflist.conf";
            Log = true;
            ConsoleLog = true;
            HandShuffle = false;
            AutoEndTurn = true;
        }

        public int ServerPort { get; private set; }
        public string Path { get; private set; }
        public string ScriptFolder { get; private set; }
        public string BanlistFile { get; private set; }
        public bool Log { get; private set; }
        public bool ConsoleLog { get; private set; }
        public bool HandShuffle { get; private set; }
        public bool AutoEndTurn { get; private set; }
        public int ClientVersion { get; private set; }

        public bool Load(string file = "config.txt")
        {
            if (!File.Exists(file)) return false;
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(File.OpenRead(file));
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null) continue;
                    line = line.Trim();
                    if (line.Equals(string.Empty)) continue;
                    if (!line.Contains("=")) continue;
                    if (line.StartsWith("#")) continue;

                    var data = line.Split(new[] {'='}, 2);
                    var variable = data[0].Trim().ToLower();
                    var value = data[1].Trim();
                    switch (variable)
                    {
                        case "serverport":
                            ServerPort = Convert.ToInt32(value);
                            break;
                        case "path":
                            Path = value;
                            break;
                        case "scriptfolder":
                            ScriptFolder = value;
                            break;
                        case "banlist":
                            BanlistFile = value;
                            break;
                        case "errorlog":
                            Log = Convert.ToBoolean(value);
                            break;
                        case "consolelog":
                            ConsoleLog = Convert.ToBoolean(value);
                            break;
                        case "handshuffle":
                            HandShuffle = Convert.ToBoolean(value);
                            break;
                        case "autoendturn":
                            AutoEndTurn = Convert.ToBoolean(value);
                            break;
                        case "clientversion":
                            ClientVersion = Convert.ToInt32(value, 16);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex);
                Debug.Assert(reader != null, "reader != null");
                reader.Close();
                return false;
            }
            reader.Close();
            if (HandShuffle)
                Logger.WriteLine("Warning: Hand shuffle requires a custom ocgcore to work.");
            return true;
        }
    }
}