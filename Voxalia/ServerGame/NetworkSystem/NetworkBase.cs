using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem
{
    public class NetworkBase
    {
        public Server TheServer;

        public Thread ListenThread;

        public Socket ListenSocket;

        public NetStringManager Strings;

        public List<Connection> Connections;

        public NetworkBase(Server tserver)
        {
            TheServer = tserver;
            Strings = new NetStringManager(TheServer);
            Connections = new List<Connection>();
        }

        public void Init()
        {
            ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // TODO: IPv4 vs. IPv6 choice (CVar?)
            ListenSocket.Bind(new IPEndPoint(IPAddress.Any, 28010)); // TODO: Port option (CVar?)
            ListenSocket.Listen(100);
            ListenThread = new Thread(new ThreadStart(ListenLoop));
            ListenThread.Name = Program.GameName + "_v" + Program.GameVersion + "_NetworkListenThread";
            ListenThread.Start();
        }

        void ListenLoop()
        {
            while (true)
            {
                try
                {
                    Socket socket = ListenSocket.Accept();
                    Connections.Add(new Connection(TheServer, socket));
                }
                catch (Exception ex)
                {
                    if (ex is ThreadAbortException)
                    {
                        throw ex;
                    }
                    SysConsole.Output(OutputType.ERROR, "Network listen: " + ex.ToString());
                }
            }
        }

        public void Tick(double delta)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i].Alive)
                {
                    Connections[i].Tick(delta);
                }
                if (!Connections[i].Alive)
                {
                    Connections.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
