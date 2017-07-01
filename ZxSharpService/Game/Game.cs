using System;
using System.Collections.Generic;
using System.IO;
using ZxSharpService.Game.Enums;
using ZxWrapper.Enums;

namespace ZxSharpService.Game
{
    public class Game
    {
        //private GameAnalyser m_analyser;
        //public Replay Replay { get; private set; }
        private readonly GameRoom _mRoom;
        private readonly int[] _mBonustime;
        private readonly int[] _mHandResult;
        private readonly int[] _mTimelimit;

        private Duel _mDuel;
        private int _mLastresponse;
        private int _mLasttick;
        private int _mStartplayer;
        private bool _mSwapped;
        private DateTime? _mTime;

        public GameConfig Config { get; }
        public bool IsTpSelect { get; private set; }
        public GameState State { get; private set; }
        public DateTime SideTimer { get; private set; }
        public DateTime TpTimer { get; private set; }
        public DateTime RpsTimer { get; private set; }
        public int TurnCount { get; set; }
        public int[] LifePoints { get; set; }
        public Player[] Players { get; private set; }

        public Player[] CurPlayers;

        public bool[] IsReady { get; private set; }
        public List<Player> Observers { get; }
        public Game(GameRoom room, GameConfig config)
        {
            Config = config;
            State = GameState.Lobby;
            LifePoints = new int[2];
            Players = new Player[2];
            IsReady = new bool[2];
            _mHandResult = new int[2];
            _mTimelimit = new int[2];
            _mBonustime = new int[2];
            Observers = new List<Player>();
            _mRoom = room;
        }

        public void SendToAll(ServerPacket packet)
        {
            SendToPlayers(packet);
            SendToObservers(packet);
        }

        public void SendToAllBut(ServerPacket packet, Player except)
        {
            foreach (var player in Players)
                if (player != null && !player.Equals(except))
                    player.Send(packet);
            foreach (var player in Observers)
                if (!player.Equals(except))
                    player.Send(packet);
        }

        public void SendToAllBut(ServerPacket packet, int except)
        {
            if (except < CurPlayers.Length)
                SendToAllBut(packet, CurPlayers[except]);
            else
                SendToAll(packet);
        }

        public void SendToPlayers(ServerPacket packet)
        {
            foreach (var player in Players)
            {
                player?.Send(packet);
            }
        }

        public void SendToObservers(ServerPacket packet)
        {
            foreach (var player in Observers)
                player.Send(packet);
        }

        public void AddPlayer(Player player)
        {
            var pos = GetAvailablePlayerPos();

            var enter = new ServerPacket(ServiceMessage.PlayerEnter);
            if (pos != -1)
            {
                Players[pos] = player;
                IsReady[pos] = false;
                player.Type = pos;

                enter.Write(0 == pos ? (byte)PlayerType.Host : (byte)PlayerType.Guest);
            }
            else
            {
                player.Type = (int)PlayerType.Observer;
                Observers.Add(player);

                enter.Write((byte)PlayerType.Observer);
            }
            enter.Write(player.Name);
            SendToAll(enter);

            SendJoinGame(player);
            SendGameConfig(player);
            SendDuelistInfo(player);
        }

        private void SendJoinGame(Player player)
        {
            var join = new ServerPacket(ServiceMessage.JoinGame);
            join.Write((byte)player.Type);
            join.Write(int.Parse(_mRoom.RoomId));
            player.Send(join);
        }

        private void SendGameConfig(Player player)
        {
            var join = new ServerPacket(ServiceMessage.GameConfig);
            join.Write((byte)Config.StartLp);
            join.Write((byte)Config.StartHand);
            join.Write((byte)Config.StartResouce);
            join.Write((byte)Config.DrawCount);
            join.Write((byte)Config.GameTimer);
            join.Write(Config.ShuffleDeck ? (byte)1 : (byte)0);
            player.Send(join);
        }

        private void SendDuelistInfo(Player player)
        {
            for (var i = 0; i < Players.Length; i++)
            {
                if (Players[i] == null) continue;
                var info = new ServerPacket(ServiceMessage.DuelistInfo);
                info.Write((byte)i);
                info.Write(IsReady[i] ? (byte)PlayerChange.Ready: (byte)PlayerChange.NotReady);
                info.Write(Players[i].Name);
                player.Send(info);
            }
        }

        public void LeaveGame(Player player)
        {
            var leave = new ServerPacket(ServiceMessage.LeaveGame);
            leave.Write((byte)player.Type);
            leave.Write(player.Name);
            SendToAll(leave);

            if (player.Type.Equals(PlayerType.Host))
            {
                _mRoom.Close();
            }
            else if (player.Type.Equals(PlayerType.Guest))
            {
                Players[1] = null;
            }
            else
            {
                Observers.Remove(player);
            }
        }

        public void KickPlayer(Player player, int pos)
        {

        }

        /// <summary>
        /// 设置选手准备状态,并广播给所有人
        /// </summary>
        public void SetDuelistState(Player player, bool ready)
        {
            var state = new ServerPacket(ServiceMessage.DuelistState);
            state.Write((byte)player.Type);
            state.Write(ready ? (byte)PlayerChange.Ready : (byte)PlayerChange.NotReady);
            SendToAll(state);
        }

        public void Chat(Player player, string msg)
        {
            var packet = new ServerPacket(ServiceMessage.Chat);
            packet.Write((short) player.Type);
            packet.Write(msg, msg.Length + 1);
            SendToAllBut(packet, player);
        }

        public void ServerMessage(string msg)
        {
            var finalmsg = "[Server] " + msg;
            var packet = new ServerPacket(ServiceMessage.Chat);
            //packet.Write((short) PlayerType.Yellow);
            packet.Write(finalmsg, finalmsg.Length + 1);
            SendToAll(packet);
        }

        public void StartGame(Player player)
        {
            // 启动本地API
            SendToAll(new ServerPacket(ServiceMessage.StartGame));
        }

        public void HandResult(Player player, int result)
        {
            //if (State != GameState.Hand)
            //    return;
            //if (player.Type == (int) PlayerType.Observer)
            //    return;
            //if (result < 1 || result > 3)
            //    return;
            //var type = player.Type;
            //if (_mHandResult[type] != 0)
            //    return;
            //_mHandResult[type] = result;
            //if (_mHandResult[0] != 0 && _mHandResult[1] != 0)
            //{
            //    var packet = new GameServerPacket(StocMessage.HandResult);
            //    packet.Write((byte) _mHandResult[0]);
            //    packet.Write((byte) _mHandResult[1]);
            //    SendToObservers(packet);

            //    packet = new GameServerPacket(StocMessage.HandResult);
            //    packet.Write((byte) _mHandResult[1]);
            //    packet.Write((byte) _mHandResult[0]);

            //    if (_mHandResult[0] == _mHandResult[1])
            //    {
            //        _mHandResult[0] = 0;
            //        _mHandResult[1] = 0;
            //        SendHand();
            //        return;
            //    }
            //    if (_mHandResult[0] == 1 && _mHandResult[1] == 2 ||
            //        _mHandResult[0] == 2 && _mHandResult[1] == 3 ||
            //        _mHandResult[0] == 3 && _mHandResult[1] == 1)
            //        _mStartplayer = 1;
            //    else
            //        _mStartplayer = 0;
            //    State = GameState.Starting;
            //    Players[_mStartplayer].Send(new GameServerPacket(StocMessage.SelectTp));
            //}
        }

        public void TpResult(Player player, bool result)
        {
            if (State != GameState.Starting)
                return;
            if (player.Type != _mStartplayer)
                return;

            _mSwapped = false;
            if (result && player.Type == 1 || !result && player.Type == 0)
            {
                _mSwapped = true;
                var temp = Players[0];
                Players[0] = Players[1];
                Players[1] = temp;
                Players[0].Type = 0;
                Players[1].Type = 1;
            }
            CurPlayers[0] = Players[0];
            CurPlayers[1] = Players[1];

            State = GameState.Duel;
            var seed = Environment.TickCount;
            //_mDuel = Duel.Create((uint) seed);
            //var rand = new Random(seed);

            //_mDuel.SetAnalyzer(m_analyser.Analyse);
            _mDuel.SetErrorHandler(HandleError);

            _mDuel.InitPlayers(Config.StartLp, Config.StartHand, Config.DrawCount);

            var opt = 0;

            //Replay = new Replay((uint) seed, IsTag);
            //Replay.Writer.WriteUnicode(Players[0].RoomId, 20);
            //Replay.Writer.WriteUnicode(Players[1].RoomId, 20);
            //if (IsTag)
            //{
            //    Replay.Writer.WriteUnicode(Players[2].RoomId, 20);
            //    Replay.Writer.WriteUnicode(Players[3].RoomId, 20);
            //}
            //Replay.Writer.Write(Config.StartLp);
            //Replay.Writer.Write(Config.StartHand);
            //Replay.Writer.Write(Config.DrawCount);
            //Replay.Writer.Write(opt);

            //for (var i = 0; i < Players.Length; i++)
            //{
            //    var dplayer = Players[i == 2 ? 3 : (i == 3 ? 2 : i)];
            //    var pid = i;
            //    Replay.Writer.Write(dplayer.Deck.Main.Count);
            //    for (var j = dplayer.Deck.Main.Count - 1; j >= 0; j--)
            //    {
            //        int id = dplayer.Deck.Main[j];
            //        if (IsTag && (i == 1 || i == 3))
            //            _mDuel.AddTagCard(id, pid, CardLocation.Deck);
            //        else
            //            _mDuel.AddCard(id, pid, CardLocation.Deck);
            //        Replay.Writer.Write(id);
            //    }
            //    Replay.Writer.Write(dplayer.Deck.Extra.Count);
            //    foreach (int id in dplayer.Deck.Extra)
            //    {
            //        _mDuel.AddCard(id, pid, CardLocation.Dynamis);
            //        Replay.Writer.Write(id);
            //    }
            //}

            var packet = new ServerPacket(GameMessage.Start);
            packet.Write((byte) 0);
            packet.Write(Config.StartLp);
            packet.Write(Config.StartLp);
            packet.Write((short) _mDuel.QueryFieldCount(0, CardLocation.Deck));
            packet.Write((short) _mDuel.QueryFieldCount(0, CardLocation.Dynamis));
            packet.Write((short) _mDuel.QueryFieldCount(1, CardLocation.Deck));
            packet.Write((short) _mDuel.QueryFieldCount(1, CardLocation.Dynamis));

            packet.SetPosition(2);
            packet.Write((byte) 1);

            packet.SetPosition(2);
            if (_mSwapped)
                packet.Write((byte) 0x11);
            else
                packet.Write((byte) 0x10);
            SendToObservers(packet);

            RefreshExtra(0);
            RefreshExtra(1);

            _mDuel.Start(opt);

            TurnCount = 0;
            LifePoints[0] = Config.StartLp;
            LifePoints[1] = Config.StartLp;
            Process();
        }

        public void Surrender(Player player, int reason, bool force = false)
        {
            if (!force)
                if (State != GameState.Duel)
                    return;
            if (player.Type == (int) PlayerType.Observer)
                return;
            var win = new ServerPacket(GameMessage.Win);
            var team = player.Type;
            win.Write((byte) (1 - team));
            win.Write((byte) reason);
            SendToAll(win);

            //MatchSaveResult(1 - team);

            EndDuel(reason == 4);
        }

        public void RefreshAll()
        {
            RefreshSquare(0);
            RefreshSquare(1);
            RefreshResource(0);
            RefreshResource(1);
            RefreshHand(0);
            RefreshHand(1);
        }

        public void RefreshSquare(int player, int flag = 0x81fff, bool useCache = true)
        {
            //var result = _mDuel.QueryFieldCard(player, CardLocation.Square, flag, useCache);
            //var update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Square);
            //update.Write(result);
            //SendToTeam(update, player);

            //update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Square);

            //var ms = new MemoryStream(result);
            //var reader = new BinaryReader(ms);
            //var writer = new BinaryWriter(ms);
            //for (var i = 0; i < 5; i++)
            //{
            //    var len = reader.ReadInt32();
            //    if (len == 4)
            //        continue;
            //    var pos = ms.Position;
            //    var raw = reader.ReadBytes(len - 4);
            //    if ((raw[11] & (int) CardPosition.FaceDown) != 0)
            //    {
            //        ms.Position = pos;
            //        writer.Write(new byte[len - 4]);
            //    }
            //}
            //update.Write(result);

            //SendToTeam(update, 1 - player);
            //SendToObservers(update);
        }

        public void RefreshResource(int player, int flag = 0x681fff, bool useCache = true)
        {
            //var result = _mDuel.QueryFieldCard(player, CardLocation.ResourceZone, flag, useCache);
            //var update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.ResourceZone);
            //update.Write(result);
            //SendToTeam(update, player);

            //update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.ResourceZone);

            //var ms = new MemoryStream(result);
            //var reader = new BinaryReader(ms);
            //var writer = new BinaryWriter(ms);
            //for (var i = 0; i < 8; i++)
            //{
            //    var len = reader.ReadInt32();
            //    if (len == 4)
            //        continue;
            //    var pos = ms.Position;
            //    var raw = reader.ReadBytes(len - 4);
            //    if ((raw[11] & (int) CardPosition.FaceDown) != 0)
            //    {
            //        ms.Position = pos;
            //        writer.Write(new byte[len - 4]);
            //    }
            //}
            //update.Write(result);

            //SendToTeam(update, 1 - player);
            //SendToObservers(update);
        }

        public void RefreshHand(int player, int flag = 0x181fff, bool useCache = true)
        {
            //var result = _mDuel.QueryFieldCard(player, CardLocation.Hand, flag | 0x100000, useCache);
            //var update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Hand);
            //update.Write(result);
            //CurPlayers[player].Send(update);

            //update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Hand);

            //var ms = new MemoryStream(result);
            //var reader = new BinaryReader(ms);
            //var writer = new BinaryWriter(ms);
            //while (ms.Position < ms.Length)
            //{
            //    var len = reader.ReadInt32();
            //    if (len == 4)
            //        continue;
            //    var pos = ms.Position;
            //    var raw = reader.ReadBytes(len - 4);
            //    if (raw[len - 8] == 0)
            //    {
            //        ms.Position = pos;
            //        writer.Write(new byte[len - 4]);
            //    }
            //}
            //update.Write(result);

            //SendToAllBut(update, player);
        }

        public void RefreshGrave(int player, int flag = 0x81fff, bool useCache = true)
        {
            //var result = _mDuel.QueryFieldCard(player, CardLocation.Grave, flag, useCache);
            //var update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Grave);
            //update.Write(result);
            //SendToAll(update);
        }

        public void RefreshExtra(int player, int flag = 0x81fff, bool useCache = true)
        {
            //var result = _mDuel.QueryFieldCard(player, CardLocation.Dynamis, flag, useCache);
            //var update = new GameServerPacket(GameMessage.UpdateData);
            //update.Write((byte) player);
            //update.Write((byte) CardLocation.Dynamis);
            //update.Write(result);
            //CurPlayers[player].Send(update);
        }

        public void RefreshSingle(int player, int location, int sequence, int flag = 0x781fff)
        {
            //var result = _mDuel.QueryCard(player, location, sequence, flag);

            //if (location == (int) CardLocation.Removed && (result[15] & (int) CardPosition.FaceDown) != 0)
            //    return;

            //var update = new GameServerPacket(GameMessage.UpdateCard);
            //update.Write((byte) player);
            //update.Write((byte) location);
            //update.Write((byte) sequence);
            //update.Write(result);
            //CurPlayers[player].Send(update);

            //if (IsTag)
            //{
            //    if ((location & (int) CardLocation.Onfield) != 0)
            //    {
            //        SendToTeam(update, player);
            //        if ((result[15] & (int) CardPosition.FaceUp) != 0)
            //            SendToTeam(update, 1 - player);
            //    }
            //    else
            //    {
            //        CurPlayers[player].Send(update);
            //        if ((location & 0x90) != 0)
            //            SendToAllBut(update, player);
            //    }
            //}
            //else
            //{
            //    if ((location & 0x90) != 0 || (location & 0x2c) != 0 && (result[15] & (int) CardPosition.FaceUp) != 0)
            //        SendToAllBut(update, player);
            //}
        }

        public int WaitForResponse()
        {
            WaitForResponse(_mLastresponse);
            return _mLastresponse;
        }

        public void WaitForResponse(int player)
        {
            _mLastresponse = player;
            CurPlayers[player].State = PlayerState.Response;
            SendToAllBut(new ServerPacket(GameMessage.Waiting), player);
            TimeStart();
            var packet = new ServerPacket(ServiceMessage.TimeLimit);
            packet.Write((byte) player);
            packet.Write((byte) 0); // C++ padding
            packet.Write((short) _mTimelimit[player]);
            SendToPlayers(packet);
        }

        public void SetResponse(int resp)
        {
            //if (!Replay.Disabled)
            //{
            //    Replay.Writer.Write((byte) 4);
            //    Replay.Writer.Write(BitConverter.GetBytes(resp));
            //    Replay.Check();
            //}

            TimeStop();
            _mDuel.SetResponse(resp);
        }

        public void SetResponse(byte[] resp)
        {
            //if (!Replay.Disabled)
            //{
            //    Replay.Writer.Write((byte) resp.Length);
            //    Replay.Writer.Write(resp);
            //    Replay.Check();
            //}

            TimeStop();
            _mDuel.SetResponse(resp);
            Process();
        }

        public void EndDuel(bool force)
        {
            if (State == GameState.Duel)
            {
                //if (!Replay.Disabled)
                //{
                //    Replay.End();
                //    byte[] replayData = Replay.GetFile();
                //    var packet = new GameServerPacket(StocMessage.Replay);
                //    packet.Write(replayData);
                //    SendToAll(packet);
                //}

                State = GameState.End;
                _mDuel.End();
            }

            if (_mSwapped)
            {
                _mSwapped = false;
                var temp = Players[0];
                Players[0] = Players[1];
                Players[1] = temp;
                Players[0].Type = 0;
                Players[1].Type = 1;
            }

            End();
        }

        public void End()
        {
            SendToAll(new ServerPacket(ServiceMessage.DuelEnd));
            _mRoom.CloseDelayed();
        }

        public void TimeReset()
        {
            _mTimelimit[0] = Config.GameTimer;
            _mTimelimit[1] = Config.GameTimer;
            _mBonustime[0] = 0;
            _mBonustime[1] = 0;
        }

        public void TimeStart()
        {
            _mTime = DateTime.UtcNow;
        }

        public void TimeStop()
        {
            if (_mTime != null)
            {
                var elapsed = DateTime.UtcNow - _mTime.Value;
                _mTimelimit[_mLastresponse] -= (int) elapsed.TotalSeconds;
                if (_mTimelimit[_mLastresponse] < 0)
                    _mTimelimit[_mLastresponse] = 0;
                _mTime = null;
            }
        }

        public void TimeTick()
        {
            if (State == GameState.Duel)
                if (_mTime != null)
                {
                    //var elapsed = DateTime.UtcNow - _mTime.Value;
                    //if ((int) elapsed.TotalSeconds > _mTimelimit[_mLastresponse])
                    //    if (m_analyser.LastMessage == GameMessage.SelectIdleCmd ||
                    //        m_analyser.LastMessage == GameMessage.SelectBattleCmd)
                    //        if (Program.Config.AutoEndTurn)
                    //            if (Players[_mLastresponse].TurnSkip == 2)
                    //            {
                    //                Surrender(Players[_mLastresponse], 3);
                    //            }
                    //            else
                    //            {
                    //                Players[_mLastresponse].State = PlayerState.None;
                    //                Players[_mLastresponse].TurnSkip++;
                    //                SetResponse(m_analyser.LastMessage == GameMessage.SelectIdleCmd ? 7 : 3);
                    //                Process();
                    //            }
                    //        else
                    //            Surrender(Players[_mLastresponse], 3);
                    //    else if (elapsed.TotalSeconds > _mTimelimit[_mLastresponse] + 30)
                    //        Surrender(Players[_mLastresponse], 3);
                }

            if (State == GameState.Starting)
                if (IsTpSelect)
                {
                    var elapsed = DateTime.UtcNow - TpTimer;

                    var currentTick = 30 - elapsed.Seconds;

                    if (currentTick == 15 || currentTick < 6)
                        if (_mLasttick != currentTick)
                        {
                            ServerMessage("You have " + currentTick + " seconds left.");
                            _mLasttick = currentTick;
                        }

                    if (elapsed.TotalMilliseconds >= 30000)
                    {
                        Surrender(Players[_mStartplayer], 3, true);
                        State = GameState.End;
                        End();
                    }
                }
            if (State != GameState.Hand) return;
            {
                var elapsed = DateTime.UtcNow - RpsTimer;
                var currentTick = 60 - elapsed.Seconds;

                if (currentTick == 30 || currentTick == 15 || currentTick < 6)
                    if (_mLasttick != currentTick)
                    {
                        ServerMessage("You have " + currentTick + " seconds left.");
                        _mLasttick = currentTick;
                    }

                if ((int) elapsed.TotalMilliseconds < 60000) return;
                if (_mHandResult[0] != 0)
                {
                    Surrender(Players[1], 3, true);
                }
                else if (_mHandResult[1] != 0)
                {
                    Surrender(Players[0], 3, true);
                }
                else
                {
                    State = GameState.End;
                    End();
                }

                if (_mHandResult[0] == 0 && _mHandResult[1] == 0)
                {
                    State = GameState.End;
                    End();
                }
                else
                {
                    Surrender(Players[1 - _mLastresponse], 3, true);
                }
            }
        }

        private int GetAvailablePlayerPos()
        {
            for (var i = 0; i < Players.Length; i++)
                if (Players[i] == null)
                    return i;
            return -1;
        }

        private void SendHand()
        {
            //RpsTimer = DateTime.UtcNow;
            //var hand = new GameServerPacket(StocMessage.SelectHand);
            //SendToPlayers(hand);
        }

        private void Process()
        {
            var result = _mDuel.Process();
            switch (result)
            {
                case -1:
                    _mRoom.Close();
                    break;
                case 2: // Game finished
                    EndDuel(false);
                    break;
            }
        }

        private void SendDuelingPlayers(Player player)
        {
            //for (var i = 0; i < Players.Length; i++)
            //{
            //    var enter = new GameServerPacket(StocMessage.PlayerEnter);
            //    var id = i;
            //    if (_mSwapped)
            //        id = 1 - i;
            //    enter.Write(Players[id].Name, 20);
            //    enter.Write((byte) i);
            //    player.Send(enter);
            //}
        }

        private void InitNewSpectator(Player player)
        {
            var deck1 = _mDuel.QueryFieldCount(0, CardLocation.Deck);
            var deck2 = _mDuel.QueryFieldCount(1, CardLocation.Deck);

            var hand1 = _mDuel.QueryFieldCount(0, CardLocation.Hand);
            var hand2 = _mDuel.QueryFieldCount(1, CardLocation.Hand);

            var packet = new ServerPacket(GameMessage.Start);
            packet.Write((byte) (_mSwapped ? 0x11 : 0x10));
            packet.Write(LifePoints[0]);
            packet.Write(LifePoints[1]);
            packet.Write((short) (deck1 + hand1));
            packet.Write((short) _mDuel.QueryFieldCount(0, CardLocation.Dynamis));
            packet.Write((short) (deck2 + hand2));
            packet.Write((short) _mDuel.QueryFieldCount(1, CardLocation.Dynamis));
            player.Send(packet);

            var draw = new ServerPacket(GameMessage.Draw);
            draw.Write((byte) 0);
            draw.Write((byte) hand1);
            for (var i = 0; i < hand1; i++)
                draw.Write(0);
            player.Send(draw);

            draw = new ServerPacket(GameMessage.Draw);
            draw.Write((byte) 1);
            draw.Write((byte) hand2);
            for (var i = 0; i < hand2; i++)
                draw.Write(0);
            player.Send(draw);

            var turn = new ServerPacket(GameMessage.NewTurn);
            turn.Write((byte) 0);
            player.Send(turn);

            InitSpectatorLocation(player, CardLocation.Square);
            //InitSpectatorLocation(player, CardLocation.ResourceZone);
            //InitSpectatorLocation(player, CardLocation.Grave);
            InitSpectatorLocation(player, CardLocation.Removed);
        }

        private void InitSpectatorLocation(Player player, CardLocation loc)
        {
            //for (var index = 0; index < 2; index++)
            //{
            //    var flag = loc == CardLocation.Square ? 0x91fff : 0x81fff;
            //    var result = _mDuel.QueryFieldCard(index, loc, flag, false);

            //    var ms = new MemoryStream(result);
            //    var reader = new BinaryReader(ms);
            //    var writer = new BinaryWriter(ms);
            //    while (ms.Position < ms.Length)
            //    {
            //        var len = reader.ReadInt32();
            //        if (len == 4)
            //            continue;
            //        var pos = ms.Position;
            //        reader.ReadBytes(len - 4);
            //        var endPos = ms.Position;

            //        ms.Position = pos;
            //        var card = new ClientCard();
            //        card.Update(reader);
            //        ms.Position = endPos;

            //        var facedown = (card.Position & (int) CardPosition.FaceDown) != 0;

            //        var move = new GameServerPacket(GameMessage.Move);
            //        move.Write(facedown ? 0 : card.Code);
            //        move.Write(0);
            //        move.Write((byte) card.Controler);
            //        move.Write((byte) card.Location);
            //        move.Write((byte) card.Sequence);
            //        move.Write((byte) card.Position);
            //        move.Write(0);
            //        player.Send(move);

            //        foreach (var material in card.Overlay)
            //        {
            //            var xyzcreate = new GameServerPacket(GameMessage.Move);
            //            xyzcreate.Write(material.Code);
            //            xyzcreate.Write(0);
            //            xyzcreate.Write((byte) index);
            //            xyzcreate.Write((byte) CardLocation.Grave);
            //            xyzcreate.Write((byte) 0);
            //            xyzcreate.Write((byte) 0);
            //            xyzcreate.Write(0);
            //            player.Send(xyzcreate);

            //            var xyzmove = new GameServerPacket(GameMessage.Move);
            //            xyzmove.Write(material.Code);
            //            xyzmove.Write((byte) index);
            //            xyzmove.Write((byte) CardLocation.Grave);
            //            xyzmove.Write((byte) 0);
            //            xyzmove.Write((byte) 0);
            //            xyzmove.Write((byte) material.Controler);
            //            xyzmove.Write((byte) material.Location);
            //            xyzmove.Write((byte) material.Sequence);
            //            xyzmove.Write((byte) material.Position);
            //            xyzmove.Write(0);
            //            player.Send(xyzmove);
            //        }

            //        if (facedown)
            //        {
            //            ms.Position = pos;
            //            writer.Write(new byte[len - 4]);
            //        }
            //    }

            //    if (loc == CardLocation.Square)
            //    {
            //        result = _mDuel.QueryFieldCard(index, loc, 0x81fff, false);
            //        ms = new MemoryStream(result);
            //        reader = new BinaryReader(ms);
            //        writer = new BinaryWriter(ms);
            //        while (ms.Position < ms.Length)
            //        {
            //            var len = reader.ReadInt32();
            //            if (len == 4)
            //                continue;
            //            var pos = ms.Position;
            //            var raw = reader.ReadBytes(len - 4);

            //            var facedown = (raw[11] & (int) CardPosition.FaceDown) != 0;
            //            if (facedown)
            //            {
            //                ms.Position = pos;
            //                writer.Write(new byte[len - 4]);
            //            }
            //        }
            //    }

            //    var update = new GameServerPacket(GameMessage.UpdateData);
            //    update.Write((byte) index);
            //    update.Write((byte) loc);
            //    update.Write(result);
            //    player.Send(update);
            //}
        }

        private void HandleError(string error)
        {
            const string log = "LuaErrors.log";
            if (File.Exists(log))
                foreach (var line in File.ReadAllLines(log))
                    if (line == error)
                        return;

            var writer = new StreamWriter(log, true);
            writer.WriteLine(error);
            writer.Close();

            var packet = new ServerPacket(ServiceMessage.Chat);
            packet.Write((short) PlayerType.Observer);
            packet.Write(error, error.Length + 1);
            SendToAll(packet);
        }

        private static IList<int> ShuffleCards(Random rand, IEnumerable<int> cards)
        {
            var shuffled = new List<int>(cards);
            for (var i = shuffled.Count - 1; i > 0; --i)
            {
                var pos = rand.Next(i + 1);
                var tmp = shuffled[i];
                shuffled[i] = shuffled[pos];
                shuffled[pos] = tmp;
            }
            return shuffled;
        }

        public void BonusTime(GameMessage message)
        {
            switch (message)
            {
                case GameMessage.Summoning:
                case GameMessage.SpSummoning:
                case GameMessage.Set:
                case GameMessage.Battle:
                    if (_mBonustime[_mLastresponse] < 300 - Config.GameTimer)
                    {
                        _mBonustime[_mLastresponse] += 10;
                        _mTimelimit[_mLastresponse] += 10;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}