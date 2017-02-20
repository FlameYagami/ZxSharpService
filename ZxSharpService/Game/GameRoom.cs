using System.Collections.Generic;

namespace ZxSharpService.Game
{
    public class GameRoom
    {
        public readonly string RoomId;

        public Game Game { get; }
        public List<GameClient> MClients { get; }
        public bool IsOpen { get; private set; }
        private bool MClosePending { get; set; }

        public GameRoom(string roomId,GameConfig config)
        {
            RoomId = roomId;
            MClients = new List<GameClient>();
            Game = new Game(this, config);
            IsOpen = true;
        }

        public void AddClient(GameClient client)
        {
            MClients.Add(client);
        }

        public void RemoveClient(GameClient client)
        {
            MClients.Remove(client);
        }

        public void Close()
        {
            IsOpen = false;
            foreach (var client in MClients)
                client.Close();
        }

        public void CloseDelayed()
        {
            foreach (var client in MClients)
                client.CloseDelayed();
            MClosePending = true;
        }

        public void HandleGame()
        {
            foreach (var user in MClients)
                user.Tick();

            Game.TimeTick();

            if (MClosePending && MClients.Count == 0)
                Close();
        }
    }
}