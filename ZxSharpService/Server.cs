﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using ZxSharpService.Game;

namespace ZxSharpService
{
    internal class Server
    {
        private readonly List<GameClient> _mClients;
        private TcpListener _mListener;

        public Server()
        {
            _mClients = new List<GameClient>();
        }

        public bool IsListening { get; private set; }

        public bool Start(int port = 0)
        {
            try
            {
                //Api.Init(Program.Config.Path, Program.Config.ScriptFolder, Program.Config.CardCDB);
                //BanlistManager.Init(Program.Config.BanlistFile);
                _mListener = new TcpListener(IPAddress.Any, port == 0 ? Program.Config.ServerPort : port);
                _mListener.Start();
                IsListening = true;
            }
            catch (SocketException)
            {
                Logger.WriteError("The " + (port == 0 ? Program.Config.ServerPort : port) + " port is currently in use.");
                return false;
            }
            catch (Exception e)
            {
                Logger.WriteError(e);
                return false;
            }

            Logger.WriteLine("Listening on port " + (port == 0 ? Program.Config.ServerPort : port));
            return true;
        }

        public void Stop()
        {
            if (!IsListening) return;
            _mListener.Stop();
            IsListening = false;

            foreach (var client in _mClients)
                client.Close();
        }

        public void Process()
        {
            GameManager.HandleRooms();

            while (IsListening && _mListener.Pending())
                _mClients.Add(new GameClient(_mListener.AcceptTcpClient()));

            var toRemove = new List<GameClient>();
            foreach (var client in _mClients)
            {
                client.Tick();
                if (!client.IsConnected || client.InGame())
                    toRemove.Add(client);
            }
            while (toRemove.Count > 0)
            {
                _mClients.Remove(toRemove[0]);
                toRemove.RemoveAt(0);
            }
        }
    }
}