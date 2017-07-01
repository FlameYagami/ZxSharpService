using System.Collections.Generic;

namespace ZxSharpService.Game
{
    public class GameRoom
    {
        public readonly string RoomId;

        public Game Game { get; }
        public List<Client> MClients { get; }
        public bool IsOpen { get; private set; }
        private bool MClosePending { get; set; }

        public GameRoom(string roomId,GameConfig config)
        {
            RoomId = roomId;
            MClients = new List<Client>();
            Game = new Game(this, config);
            IsOpen = true;
        }

        public void AddClient(Client client)
        {
            MClients.Add(client);
        }

        public void RemoveClient(Client client)
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