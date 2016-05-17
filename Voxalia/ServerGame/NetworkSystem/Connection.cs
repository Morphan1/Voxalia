﻿using System;
using System.Net.Sockets;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsIn;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Files;
using FreneticScript;

namespace Voxalia.ServerGame.NetworkSystem
{
    public class Connection
    {
        public Server TheServer;

        public Socket PrimarySocket;

        public Connection(Server tserver, Socket psocket)
        {
            TheServer = tserver;
            PrimarySocket = psocket;
            PrimarySocket.Blocking = true;// false;
            recd = new byte[MAX];
        }

        int MAX = 1024 * 1024; // 1 MB by default

        byte[] recd;

        int recdsofar = 0;

        bool GotBase = false;

        public double Time = 0;

        public double MaxTime = 10; // Maximum connection time without GotBase

        public bool Alive = true;

        PlayerEntity PE;

        public void SendMessage(string message)
        {
            SendPacket(new MessagePacketOut(message));
        }

        public void SendPacket(AbstractPacketOut packet)
        {
            try
            {
                if (Alive)
                {
                    byte id = (byte)packet.ID;
                    byte[] data = packet.Data;
                    byte[] fdata = new byte[data.Length + 5];
                    Utilities.IntToBytes(data.Length).CopyTo(fdata, 0);
                    fdata[4] = id;
                    data.CopyTo(fdata, 5);
                    PrimarySocket.Send(fdata);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.WARNING, "Disconnected " + PE + " -> " + ex.GetType().Name + ": " + ex.Message);
                // TODO: Debug Only
                SysConsole.Output(ex);
                PE.Kick("Internal exception.");
            }
        }
        
        public void Tick(double delta)
        {
            try
            {
                Time += delta;
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
                        ClientToServerPacket packetID = (ClientToServerPacket)recd[4];
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
                            case ClientToServerPacket.PING:
                                packet = new PingPacketIn();
                                break;
                            case ClientToServerPacket.KEYS:
                                packet = new KeysPacketIn();
                                break;
                            case ClientToServerPacket.COMMAND:
                                packet = new CommandPacketIn();
                                break;
                            case ClientToServerPacket.HOLD_ITEM:
                                packet = new HoldItemPacketIn();
                                break;
                            case ClientToServerPacket.DISCONNECT:
                                packet = new DisconnectPacketIn();
                                break;
                            case ClientToServerPacket.SET_STATUS:
                                packet = new SetStatusPacketIn();
                                break;
                            case ClientToServerPacket.PLEASE_REDEFINE:
                                packet = new PleaseRedefinePacketIn();
                                break;
                            default:
                                throw new Exception("Invalid packet ID: " + packetID);
                        }
                        packet.Chunk = PE.ChunkNetwork == this;
                        packet.Player = PE;
                        if (!packet.ParseBytesAndExecute(data))
                        {
                            throw new Exception("Imperfect packet data for " + packetID);
                        }
                    }
                }
                else
                {
                    if (recd[0] == 'G' && recd[1] == 'E' && recd[2] == 'T' && recd[3] == ' ' && recd[4] == '/')
                    {
                        // HTTP GET
                        if (recd[recdsofar - 1] == '\n' && (recd[recdsofar - 2] == '\n' || recd[recdsofar - 3] == '\n'))
                        {
                            WebPage wp = new WebPage(TheServer, this);
                            wp.Init(FileHandler.encoding.GetString(recd, 0, recdsofar));
                            PrimarySocket.Send(wp.GetFullData());
                            PrimarySocket.Close();
                            Alive = false;
                        }
                    }
                    else if (recd[0] == 'P' && recd[1] == 'O' && recd[2] == 'S' && recd[3] == 'T' && recd[4] == ' ')
                    {
                        // HTTP POST
                        throw new NotImplementedException("HTTP POST not yet implemented");
                    }
                    else if (recd[0] == 'H' && recd[1] == 'E' && recd[2] == 'A' && recd[3] == 'D' && recd[4] == ' ')
                    {
                        // HTTP HEAD
                        if (recd[recdsofar - 1] == '\n' && (recd[recdsofar - 2] == '\n' || recd[recdsofar - 3] == '\n'))
                        {
                            WebPage wp = new WebPage(TheServer, this);
                            wp.Init(FileHandler.encoding.GetString(recd, 0, recdsofar));
                            PrimarySocket.Send(FileHandler.encoding.GetBytes(wp.GetHeaders()));
                            PrimarySocket.Close();
                            Alive = false;
                        }
                    }
                    else if (recd[0] == 'V' && recd[1] == 'O' && recd[2] == 'X' && recd[3] == 'p' && recd[4] == '_')
                    {
                        // VOXALIA ping
                        throw new NotImplementedException("VOXALIA ping not yet implemented");
                        /*if (recd[recdsofar - 1] == '\n')
                        {
                        }*/
                    }
                    else if (recd[0] == 'V' && recd[1] == 'O' && recd[2] == 'X' && recd[3] == '_' && recd[4] == '_')
                    {
                        // VOXALIA connect
                        if (recd[recdsofar - 1] == '\n')
                        {
                            string data = FileHandler.encoding.GetString(recd, 6, recdsofar - 6);
                            string[] datums = data.SplitFast('\r');
                            if (datums.Length != 4)
                            {
                                throw new Exception("Invalid VOX__ connection details!");
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
                            PrimarySocket.Send(FileHandler.encoding.GetBytes("ACCEPT\n"));
                            PlayerEntity player = new PlayerEntity(TheServer.LoadedRegions[0], this, name);
                            player.Host = host;
                            player.Port = port;
                            player.IP = PrimarySocket.RemoteEndPoint.ToString();
                            TheServer.PlayersWaiting.Add(player);
                            GotBase = true;
                            PE = player;
                            recdsofar = 0;
                        }
                    }
                    else if (recd[0] == 'V' && recd[1] == 'O' && recd[2] == 'X' && recd[3] == 'c' && recd[4] == '_')
                    {
                        // VOXALIA chunk connect
                        if (recd[recdsofar - 1] == '\n')
                        {
                            string data = FileHandler.encoding.GetString(recd, 6, recdsofar - 6);
                            string[] datums = data.SplitFast('\r');
                            if (datums.Length != 4)
                            {
                                throw new Exception("Invalid VOXc_ connection details!");
                            }
                            string name = datums[0];
                            string key = datums[1];
                            string host = datums[2];
                            string port = datums[3];
                            if (!Utilities.ValidateUsername(name))
                            {
                                throw new Exception("Invalid connection - unreasonable username!");
                            }
                            PlayerEntity player = null;
                            for (int i = 0; i < TheServer.PlayersWaiting.Count; i++)
                            {
                                if (TheServer.PlayersWaiting[i].Name == name && TheServer.PlayersWaiting[i].Host == host &&
                                    TheServer.PlayersWaiting[i].Port == port)
                                    // TODO: Check key
                                {
                                    player = TheServer.PlayersWaiting[i];
                                    TheServer.PlayersWaiting.RemoveAt(i);
                                    break;
                                }
                            }
                            if (player == null)
                            {
                                throw new Exception("Can't find player for VOXc_!");
                            }
                            player.ChunkNetwork = this;
                            PrimarySocket.Send(FileHandler.encoding.GetBytes("ACCEPT\n"));
                            player.TheRegion.SpawnEntity(player);
                            player.LastPingByte = 0;
                            player.LastCPingByte = 0;
                            PrimarySocket.SendBufferSize *= 10;
                            SendPacket(new PingPacketOut(0));
                            player.Network.SendPacket(new PingPacketOut(0));
                            player.SendStatus();
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
                try
                {
                    if (PE != null)
                    {
                        PE.Kick("Internal exception.");
                    }
                }
                finally
                {
                    Utilities.CheckException(ex);
                    SysConsole.Output(OutputType.WARNING, "Forcibly disconnected client: " + ex.GetType().Name + ": " + ex.Message);
                    // TODO: Debug Only
                    SysConsole.Output(ex);
                    Alive = false;
                }
            }
        }
    }
}
