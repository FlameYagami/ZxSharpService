using System;
using System.Collections.Generic;
using System.Linq;
using ZxSharpService.Game;
using ZxSharpService.Game.Enums;

namespace ZxSharpService
{
    internal class GameManager
    {
        private static readonly Dictionary<string, GameRoom> RoomDic = new Dictionary<string, GameRoom>();

        public static GameRoom CreateGame(GameConfig config)
        {
            var roomId = GetRandomRoomId();
            if (null == roomId) return null;
            var room = new GameRoom(roomId, config);
            RoomDic.Add(roomId, room);
            return room;
        }

        public static GameRoom GetGame(string roomId)
        {
            return RoomDic.ContainsKey(roomId) ? RoomDic[roomId] : null;
        }

        public static bool IsGameExists(string roomId)
        {
            return RoomDic.ContainsKey(roomId);
        }

        public static GameRoom SpectateRandomGame()
        {
            var rooms = new GameRoom[RoomDic.Count];
            RoomDic.Values.CopyTo(rooms, 0);
            var filteredRooms = rooms.Where(room => room.Game.State != GameState.Lobby).ToList();
            return filteredRooms.Count == 0 ? null : filteredRooms[Program.Random.Next(0, filteredRooms.Count)];
        }

       /// <summary>
       /// 刷新房间
       /// </summary>
        public static void RefreshRooms()
        {
            var toRemove = new List<string>();
            foreach (var room in RoomDic)
                if (room.Value.IsOpen)
                    room.Value.HandleGame();
                else
                    toRemove.Add(room.Key);

            foreach (var room in toRemove)
            {
                RoomDic.Remove(room);
                Logger.WriteLine("Game--");
            }
        }

        /// <summary>
        /// 获取随机的房间编号
        /// </summary>
        /// <returns></returns>
        public static string GetRandomRoomId()
        {
            while (true)
            {
                var roomId = new Random().Next(100000, 999999).ToString();
                if (RoomDic.Count >= 890000)
                {
                    return null;
                }
                if (!RoomDic.ContainsKey(roomId))
                {
                    return roomId;
                }
            }
        }
    }
}