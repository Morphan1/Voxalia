using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.NetworkSystem
{
    public class Connection
    {
        public Server TheServer;

        public Socket PrimarySocket;

        public Connection(Server tserver, Socket psocket)
        {
            TheServer = tserver;
            PrimarySocket = psocket;
            PrimarySocket.Blocking = false;
            recd = new byte[MAX];
        }

        int MAX = 1024 * 20; // 20 KB by default

        byte[] recd;

        int recdsofar = 0;

        bool GotBase = false;

        public double Time;

        public double MaxTime = 10; // Maximum connection time without GotBase

        public bool Alive = true;

        public void Tick()
        {
            try
            {
                Time += TheServer.Delta;
                if (!GotBase && Time >= MaxTime)
                {
                    throw new Exception("Connection timed out!");
                }
                int avail = PrimarySocket.Available;
                if (avail <= 0)
                {
                    return;
                }
                if (avail + recdsofar > MAX)
                {
                    throw new Exception("Received too much data!");
                }
                byte[] newdata = new byte[avail];
                PrimarySocket.Receive(newdata, avail, SocketFlags.None);
                Array.Copy(newdata, 0, recd, recdsofar, avail);
                recdsofar += avail;
                if (recdsofar < 5)
                {
                    return;
                }
                if (GotBase)
                {
                }
                else
                {
                    if (recd[0] == 'G' && recd[1] == 'E' && recd[2] == 'T' && recd[3] == ' ' && recd[4] == '/')
                    {
                        // HTTP GET
                        throw new NotImplementedException("HTTP GET not yet implemented");
                    }
                    else if (recd[0] == 'P' && recd[1] == 'O' && recd[2] == 'S' && recd[3] == 'T' && recd[4] == ' ')
                    {
                        // HTTP POST
                        throw new NotImplementedException("HTTP POST not yet implemented");
                    }
                    else if (recd[0] == 'H' && recd[1] == 'E' && recd[2] == 'A' && recd[3] == 'D' && recd[4] == ' ')
                    {
                        // HTTP HEAD
                        throw new NotImplementedException("HTTP HEAD not yet implemented");
                    }
                    else if (recd[0] == 'S' && recd[1] == 'O' && recd[2] == 'G' && recd[3] == '_' && recd[4] == '_')
                    {
                    }
                    else
                    {
                        throw new Exception("Unknown initial byte set!");
                    }
                }
            }
            catch (Exception ex)
            {
                PrimarySocket.Close();
                SysConsole.Output(OutputType.WARNING, "Forcibly disconnected client: " + ex.GetType().Name + ": " + ex.Message);
                Alive = false;
            }
        }
    }
}
