using System;

namespace ZxSharpService.Game
{
    public class GameConfig
    {
        public GameConfig()
        {
            StartLp = 4;
            StartHand = 4;
            StartResouce = 2;
            DrawCount = 2;
            GameTimer = 120;
        }

        public GameConfig(GameClientPacket packet)
        {
        }

        public int StartLp { get; private set; }
        public int StartResouce { get; private set; }
        public int StartHand { get; private set; }
        public int DrawCount { get; private set; }
        public int GameTimer { get; private set; }
    }
}