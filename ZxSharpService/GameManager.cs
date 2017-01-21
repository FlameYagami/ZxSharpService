using System;
using System.Collections.Generic;
using System.Linq;
using ZxSharpService.Game;
using ZxSharpService.Game.Enums;

namespace ZxSharpService
{
    internal class GameManager
    {
        private static readonly Dictionary<string, GameRoom> MRooms = new Dictionary<string, GameRoom>();

        public static GameRoom CreateOrGetGame(GameConfig config)
        {
            if (MRooms.ContainsKey(config.Name))
                return MRooms[config.Name];
            return CreateRoom(config);
        }

        public static GameRoom GetGame(string name)
        {
            if (MRooms.ContainsKey(name))
                return MRooms[name];
            return null;
        }

        public static GameRoom GetRandomGame(int filter = -1)
        {
            var filteredRooms = new List<GameRoom>();
            var rooms = new GameRoom[MRooms.Count];
            MRooms.Values.CopyTo(rooms, 0);

            //foreach (var room in rooms)
            //    if (room.Game.State == GameState.Lobby
            //        && (filter == -1 ? true : room.Game.Config.Rule == filter))
            //        filteredRooms.Add(room);

            return filteredRooms.Count == 0 ? null : filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

        public static GameRoom SpectateRandomGame()
        {
            var rooms = new GameRoom[MRooms.Count];
            MRooms.Values.CopyTo(rooms, 0);
            var filteredRooms = rooms.Where(room => room.Game.State != GameState.Lobby).ToList();
            return filteredRooms.Count == 0 ? null : filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

        private static GameRoom CreateRoom(GameConfig config)
        {
            var room = new GameRoom(config);
            MRooms.Add(config.Name, room);
            Logger.WriteLine("Game++");
            return room;
        }

        public static void HandleRooms()
        {
            var toRemove = new List<string>();
            foreach (var room in MRooms)
                if (room.Value.IsOpen)
                    room.Value.HandleGame();
                else
                    toRemove.Add(room.Key);

            foreach (var room in toRemove)
            {
                MRooms.Remove(room);
                Logger.WriteLine("Game--");
            }
        }

        public static bool GameExists(string name)
        {
            return MRooms.ContainsKey(name);
        }

        public static string RandomRoomName()
        {
            while (true) //keep searching till one is found!!
            {
                var GuidString = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
                GuidString = GuidString.Replace("=", "");
                GuidString = GuidString.Replace("+", "");
                var roomname = GuidString.Substring(0, 5);
                if (!MRooms.ContainsKey(roomname))
                    return roomname;
            }
        }
    }
}