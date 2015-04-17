using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsIn;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

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

        public double Time = 0;

        public double MaxTime = 10; // Maximum connection time without GotBase

        public bool Alive = true;

        PlayerEntity PE;

        public void SendPacket(AbstractPacketOut packet)
        {
            byte id = packet.ID;
            byte[] data = packet.Data;
            byte[] fdata = new byte[data.Length + 5];
            Utilities.IntToBytes(data.Length).CopyTo(fdata, 0);
            fdata[4] = id;
            data.CopyTo(fdata, 5);
            PrimarySocket.Send(fdata);
        }

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
                PrimarySocket.Receive(recd, recdsofar, avail, SocketFlags.None);
                recdsofar += avail;
                if (recdsofar < 5)
                {
                    return;
                }
                if (GotBase)
                {
                    while (true)
                    {
                        byte[] len_bytes = new byte[4];
                        Array.Copy(recd, len_bytes, 4);
                        int len = Utilities.BytesToInt(len_bytes);
                        if (len + 5 > MAX)
                        {
                            throw new Exception("Unreasonably huge packet!");
                        }
                        if (recdsofar < 5 + len)
                        {
                            return;
                        }
                        byte packetID = recd[4];
                        byte[] data = new byte[len];
                        Array.Copy(recd, 5, data, 0, len);
                        byte[] rem_data = new byte[recdsofar - (len + 5)];
                        if (rem_data.Length > 0)
                        {
                            Array.Copy(recd, len + 5, rem_data, 0, rem_data.Length);
                            Array.Copy(rem_data, recd, rem_data.Length);
                        }
                        recdsofar -= len + 5;
                        AbstractPacketIn packet;
                        switch (packetID) // TODO: Packet registry?
                        {
                            case 0:
                                packet = new PingPacketIn();
                                break;
                            case 1:
                                packet = new KeysPacketIn();
                                break;
                            default:
                                throw new Exception("Invalid packet ID!");
                        }
                        packet.Player = PE;
                        if (!packet.ParseBytesAndExecute(data))
                        {
                            throw new Exception("Imperfect packet data!");
                        }
                    }
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
                        if (recd[recdsofar - 1] == '\n')
                        {
                            string data = FileHandler.encoding.GetString(recd, 6, recdsofar - 6);
                            string[] datums = data.Split('\r');
                            if (datums.Length != 4)
                            {
                                throw new Exception("Invalid SOG__ connection details!");
                            }
                            string name = datums[0];
                            string key = datums[1];
                            string host = datums[2];
                            string port = datums[3];
                            if (!Utilities.ValidateUsername(name))
                            {
                                throw new Exception("Invalid connection - unreasonable username!");
                            }
                            // TODO: Additional details?
                            PlayerEntity player = new PlayerEntity(TheServer, this);
                            player.Name = name;
                            player.Host = host;
                            player.Port = port;
                            player.IP = PrimarySocket.RemoteEndPoint.ToString();
                            PrimarySocket.Send(FileHandler.encoding.GetBytes("ACCEPT\n"));
                            TheServer.SpawnEntity(player);
                            player.LastPingByte = 0;
                            SendPacket(new PingPacketOut(0));
                            GotBase = true;
                            PE = player;
                            recdsofar = 0;
                        }
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
