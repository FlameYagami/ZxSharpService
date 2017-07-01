using System;

namespace ZxSharpService.Game
{
    public class GameConfig
    {
        public int StartLp { get; private set; }
        public int StartHand { get; private set; }
        public int StartResouce { get; private set; }
        public int DrawCount { get; private set; }
        public int GameTimer { get; private set; }
        public bool ShuffleDeck { get; private set; }

        public GameConfig()
        {
            StartLp = 4;
            StartHand = 4;
            StartResouce = 2;
            DrawCount = 2;
            GameTimer = 120;
            ShuffleDeck = true;
        }

        public GameConfig(ClientPacket packet)
        {
            StartLp = packet.ReadByte();
            StartHand = packet.ReadByte();
            StartResouce = packet.ReadByte();
            DrawCount = packet.ReadByte();
            GameTimer = packet.ReadByte();
            ShuffleDeck = packet.ReadByte() == 0;
        }
    }
}