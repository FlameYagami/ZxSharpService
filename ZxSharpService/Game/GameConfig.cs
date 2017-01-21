﻿using System;

namespace ZxSharpService.Game
{
    public class GameConfig
    {
        public GameConfig(string info)
        {
            if (info.ToLower() == "tcg" || info.ToLower() == "ocg" || info.ToLower() == "ocg/tcg" ||
                info.ToLower() == "tcg/ocg")
            {
                //LfList = info.ToLower() == "ocg/tcg" ? 1 : info.ToLower() == "tcg/ocg" ? 0 : info.ToLower() == "ocg" ? 1 : 0;
                //Rule = info.ToLower() == "ocg/tcg" || info.ToLower() == "tcg/ocg" ? 2 : info.ToLower() == "tcg" ? 1 : 0;
                //Mode = 0;
                //EnablePriority = false;
                //NoCheckDeck = false;
                //NoShuffleDeck = false;
                StartLp = 4;
                StartHand = 4;
                DrawCount = 2;
                GameTimer = 120;
                Name = GameManager.RandomRoomName();
            }
            else
            {
                Load(info);
            }
        }

        public GameConfig(GameClientPacket packet)
        {
            //LfList = BanlistManager.GetIndex(packet.ReadUInt32());
            //Rule = packet.ReadByte();
            //Mode = packet.ReadByte();
            //EnablePriority = Convert.ToBoolean(packet.ReadByte());
            //NoCheckDeck = Convert.ToBoolean(packet.ReadByte());
            //NoShuffleDeck = Convert.ToBoolean(packet.ReadByte());
            //C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (var i = 0; i < 3; i++)
                packet.ReadByte();
            StartLp = packet.ReadInt32();
            StartHand = packet.ReadByte();
            DrawCount = packet.ReadByte();
            GameTimer = packet.ReadInt16();
            packet.ReadUnicode(20);
            Name = packet.ReadUnicode(30);
            if (string.IsNullOrEmpty(Name))
                Name = GameManager.RandomRoomName();
        }

        //public int LfList { get; private set; }
        //public int Rule { get; private set; }
        //public int Mode { get; private set; }
        //public bool EnablePriority { get; private set; }
        //public bool NoCheckDeck { get; private set; }
        //public bool NoShuffleDeck { get; private set; }
        public int StartLp { get; private set; }
        public int StartHand { get; private set; }
        public int DrawCount { get; private set; }
        public int GameTimer { get; private set; }
        public string Name { get; private set; }

        public void Load(string gameinfo)
        {
            try
            {
                var rules = gameinfo.Substring(0, 6);

                //Rule = int.Parse(rules[0].ToString());
                //Mode = int.Parse(rules[1].ToString());
                GameTimer = int.Parse(rules[2].ToString()) == 0 ? 120 : 60;
                //EnablePriority = rules[3] == 'T' || rules[3] == '1';
                //NoCheckDeck = rules[4] == 'T' || rules[4] == '1';
                //NoShuffleDeck = rules[5] == 'T' || rules[5] == '1';

                var data = gameinfo.Substring(6, gameinfo.Length - 6);

                var list = data.Split(',');

                StartLp = int.Parse(list[0]);
                //LfList = int.Parse(list[1]);

                StartHand = int.Parse(list[2]);
                DrawCount = int.Parse(list[3]);

                Name = list[4];
            }
            catch (Exception)
            {
                //LfList = 0;
                //Rule = 2;
                //Mode = 0;
                //EnablePriority = false;
                //NoCheckDeck = false;
                //NoShuffleDeck = false;
                StartLp = 4;
                StartHand = 4;
                DrawCount = 2;
                GameTimer = 120;
                Name = GameManager.RandomRoomName();
            }
        }
    }
}