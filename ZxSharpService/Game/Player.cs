using System.Runtime.CompilerServices;
using System.Text;
using ZxSharpService.Game.Enums;
using ZxSharpService.Helper;

namespace ZxSharpService.Game
{
    public class Player
    {
        private readonly GameClient _mClient;
        public Game Game { get; private set; }
        public string Name { get; private set; }
        public int Type { get; set; }
        public PlayerState State { get; set; }
        // 玩家网络状态
        public bool IsAuthentified { get; private set; }

        public Player(GameClient client)
        {
            Game = client.Game;
            Type = (int) PlayerType.Undefined;
            State = PlayerState.None;
            _mClient = client;
        }

        public void Send(GameServerPacket packet)
        {
            _mClient.Send(packet.GetBytes());
        }

        public void Disconnect()
        {
            _mClient.Close();
        }

        public void OnDisconnected()
        {
            if (IsAuthentified)
                Game.RemovePlayer(this);
        }

        public void SendTypeChange()
        {
            var packet = new GameServerPacket(StocMessage.TypeChange);
            //packet.Write((byte) (Type + (Game.HostPlayer.Equals(this) ? (int) PlayerType.Host : 0)));
            Send(packet);
        }

        public bool Equals(Player player)
        {
            return ReferenceEquals(this, player);
        }

        public void Parse(GameClientPacket packet)
        {
            // 关于指令处理
            var devType = packet.ReadDevType();
            // 关于指令处理
            var cmd = packet.ReadCmd();
            switch (cmd)
            {
                case CmdMessage.CreateGame:
                    CreateGame(packet);
                    break;
                case CmdMessage.JoinGame:
                    JoinGame(packet);
                    break;
                case CmdMessage.LeaveGame:
                    Game.RemovePlayer(this);
                    break;
            }
            //switch (msg)
            //{
            //    case InstructMessage.Chat:
            //        OnChat(packet);
            //        break;
            //    case InstructMessage.HsToDuelist:
            //        Game.MoveToDuelist(this);
            //        break;
            //    case InstructMessage.HsToObserver:
            //        Game.MoveToObserver(this);
            //        break;
            //    case InstructMessage.HsReady:
            //        Game.SetReady(this, true);
            //        break;
            //    case InstructMessage.HsNotReady:
            //        Game.SetReady(this, false);
            //        break;
            //    case InstructMessage.HsKick:
            //        OnKick(packet);
            //        break;
            //    case InstructMessage.HsStart:
            //        Game.StartDuel(this);
            //        break;
            //    case InstructMessage.HandResult:
            //        OnHandResult(packet);
            //        break;
            //    case InstructMessage.TpResult:
            //        OnTpResult(packet);
            //        break;
            //    case InstructMessage.Response:
            //        OnResponse(packet);
            //        break;
            //    case InstructMessage.Surrender:
            //        Game.Surrender(this, 0);
            //        break;
            //}
        }

        private void CreateGame(GameClientPacket packet)
        {
            var room = GameManager.CreateGame(new GameConfig());
            if (null != room)
            {
                IsAuthentified = true;
                Game = room.Game;
                Name = packet.ReadStringToEnd();
                Game.AddPlayer(this);
            }
        }

        private void JoinGame(GameClientPacket packet)
        {
            GameRoom room = null;
            var data = packet.ReadStringToEnd().Split('#');
            var name = data[0];
            var roomId = data[1];

            if (GameManager.IsGameExists(roomId))
            {
                room = GameManager.GetGame(roomId);
            }
            if (null != room)
            {
                IsAuthentified = false;
                Game = room.Game;
                Name = name;
                Game.AddPlayer(this);
            }  
        }

        private void OnChat(GameClientPacket packet)
        {
            //var msg = packet.ReadUnicode(256);
            //Game.Chat(this, msg);
        }

        private void OnKick(GameClientPacket packet)
        {
            //int pos = packet.ReadByte();
            //Game.KickPlayer(this, pos);
        }

        private void OnHandResult(GameClientPacket packet)
        {
            //int res = packet.ReadByte();
            //Game.HandResult(this, res);
        }

        private void OnTpResult(GameClientPacket packet)
        {
            //var tp = packet.ReadByte() != 0;
            //Game.TpResult(this, tp);
        }

        private void OnResponse(GameClientPacket packet)
        {
            //if (Game.State != GameState.Duel)
            //    return;
            //if (State != PlayerState.Response)
            //    return;
            //var resp = packet.ReadBytesToEnd();
            //if (resp.Length > 64)
            //    return;
            //State = PlayerState.None;
            //Game.SetResponse(resp);
        }

        private void LobbyError(string message)
        {
            var join = new GameServerPacket(StocMessage.JoinGame);
            join.Write(0U);
            join.Write((byte) 0);
            join.Write((byte) 0);
            join.Write(0);
            join.Write(0);
            join.Write(0);
            // C++ padding: 5 bytes + 3 bytes = 8 bytes
            for (var i = 0; i < 3; i++)
                join.Write((byte) 0);
            join.Write(0);
            join.Write((byte) 0);
            join.Write((byte) 0);
            join.Write((short) 0);
            Send(join);

            var enter = new GameServerPacket(StocMessage.HsPlayerEnter);
            enter.Write("[" + message + "]", 20);
            enter.Write((byte) 0);
            Send(enter);
        }

        private void ServerMessage(string msg)
        {
            var finalmsg = "[Server] " + msg;
            var packet = new GameServerPacket(StocMessage.Chat);
            //packet.Write((short) PlayerType.Yellow);
            packet.Write(finalmsg, finalmsg.Length + 1);
            Send(packet);
        }
    }
}