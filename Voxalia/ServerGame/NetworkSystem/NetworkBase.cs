using System;
using System.Collections.Generic;
using Voxalia.ServerGame.ServerMainSystem;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Voxalia.Shared;
using Open.Nat;

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
            try
            {
                NatDiscoverer natdisc = new NatDiscoverer();
                NatDevice natdev = natdisc.DiscoverDeviceAsync().Result;
                Mapping map = natdev.GetSpecificMappingAsync(Protocol.Tcp, TheServer.Port).Result;
                if (map != null)
                {
                    natdev.DeletePortMapAsync(map).Wait();
                }
                natdev.CreatePortMapAsync(new Mapping(Protocol.Tcp, TheServer.Port, TheServer.Port, "Voxalia")).Wait();
                map = natdev.GetSpecificMappingAsync(Protocol.Tcp, TheServer.Port).Result;
                SysConsole.Output(OutputType.INIT, "Successfully opened server to public address " + map.PrivateIP + " or " + map.PublicIP + ", with port " + map.PrivatePort + " or " + map.PublicPort + ", as " + map.Description);
            }
            catch (Exception ex)
            {
                SysConsole.Output("Trying to open port " + TheServer.Port, ex);
            }
            if (Socket.OSSupportsIPv6)
            {
                try
                {
                    ListenSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                    ListenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27 /* IPv6Only */, false);
                    ListenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, TheServer.Port));
                }
                catch (Exception ex)
                {
                    SysConsole.Output("Opening IPv6/IPv4 combo-socket", ex);
                    ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    ListenSocket.Bind(new IPEndPoint(IPAddress.Any, TheServer.Port));
                }
            }
            else
            {
                ListenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ListenSocket.Bind(new IPEndPoint(IPAddress.Any, TheServer.Port));
            }
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
                    Utilities.CheckException(ex);
                    SysConsole.Output(OutputType.ERROR, "Network listen: " + ex.ToString());
                }
            }
        }

        public void Tick(double delta)
        {
            for (int i = 0; i < Connections.Count; i++)
            {
                if (Connections[i] == null)
                {
                    Connections.RemoveAt(i);
                    i--;
                }
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
