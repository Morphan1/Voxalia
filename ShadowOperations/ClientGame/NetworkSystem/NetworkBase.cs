using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsIn;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ClientGame.NetworkSystem
{
    public class NetworkBase
    {
        public Client TheClient;

        public NetworkBase(Client tclient)
        {
            TheClient = tclient;
            Strings = new NetStringManager();
            recd = new byte[MAX];
        }

        public NetStringManager Strings;

        public Socket ConnectionSocket;

        public Thread ConnectionThread;

        public string LastIP;

        public string LastPort;

        public bool IsAlive = false;

        public void Disconnect()
        {
            if (ConnectionThread != null)
            {
                ConnectionThread.Abort();
                ConnectionThread = null;
            }
            // TODO: Send disconnect packet
            if (ConnectionSocket != null)
            {
                ConnectionSocket.Close(2);
                ConnectionSocket = null;
            }
            IsAlive = false;
        }

        public void Connect(string IP, string port)
        {
            Disconnect();
            Strings.Strings.Clear();
            TheClient.ResetWorld();
            LastIP = IP;
            LastPort = port;
            ConnectionThread = new Thread(new ThreadStart(ConnectInternal));
            ConnectionThread.Name = Program.GameVersion + "_v" + Program.GameVersion + "_NetworkConnectionThread";
            ConnectionThread.Start();
        }

        int MAX = 1024 * 1024 * 2; // 2 MB by default

        byte[] recd;

        int recdsofar = 0;

        public void Tick()
        {
            // TODO: Connection timeout
            if (!IsAlive)
            {
                return;
            }
            try
            {
                int avail = ConnectionSocket.Available;
                if (avail <= 0)
                {
                    return;
                }
                if (avail + recdsofar > MAX)
                {
                    throw new Exception("Received too much data!");
                }
                ConnectionSocket.Receive(recd, recdsofar, avail, SocketFlags.None);
                recdsofar += avail;
                if (recdsofar < 5)
                {
                    return;
                }
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
                            packet = new YourPositionPacketIn();
                            break;
                        case 2:
                            packet = new SpawnPhysicsEntityPacketIn();
                            break;
                        case 3:
                            packet = new PhysicsEntityUpdatePacketIn();
                            break;
                        case 4:
                            packet = new SpawnLightPacketIn();
                            break;
                        case 5:
                            packet = new MessagePacketIn();
                            break;
                        case 6:
                            packet = new PlayerUpdatePacketIn();
                            break;
                        case 7:
                            packet = new SpawnBulletPacketIn();
                            break;
                        case 8:
                            packet = new DespawnEntityPacketIn();
                            break;
                        case 9:
                            packet = new NetStringPacketIn();
                            break;
                        case 10:
                            packet = new SpawnItemPacketIn();
                            break;
                        case 11:
                            packet = new YourStatusPacketIn();
                            break;
                        case 12:
                            packet = new AddJointPacketIn();
                            break;
                        case 13:
                            packet = new YourEIDPacketIn();
                            break;
                        case 14:
                            packet = new DestroyJointPacketIn();
                            break;
                        case 15:
                            packet = new SpawnPrimitiveEntityPacketIn();
                            break;
                        case 16:
                            packet = new PrimitiveEntityUpdatePacketIn();
                            break;
                        case 17:
                            packet = new AnimationPacketIn();
                            break;
                        case 18:
                            packet = new FlashLightPacketIn();
                            break;
                        case 19:
                            packet = new RemoveItemPacketIn();
                            break;
                        default:
                            throw new Exception("Invalid packet ID: " + packetID);
                    }
                    packet.TheClient = TheClient;
                    if (!packet.ParseBytesAndExecute(data))
                    {
                        throw new Exception("Imperfect packet data for packet " + packetID);
                    }
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Forcibly disconnected from server: " + ex.GetType().Name + ": " + ex.Message);
                SysConsole.Output(OutputType.INFO, ex.ToString()); // TODO: Make me 'debug only'!
                Disconnect();
            }
        }

        public void SendPacket(AbstractPacketOut packet)
        {
            if (!IsAlive)
            {
                return;
            }
            try
            {
                byte id = packet.ID;
                byte[] data = packet.Data;
                byte[] fdata = new byte[data.Length + 5];
                Utilities.IntToBytes(data.Length).CopyTo(fdata, 0);
                fdata[4] = id;
                data.CopyTo(fdata, 5);
                ConnectionSocket.Send(fdata);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.WARNING, "Forcibly disconnected from server: " + ex.GetType().Name + ": " + ex.Message);
                Disconnect();
            }
        }

        void ConnectInternal()
        {
            try
            {
                string key = Utilities.UtilRandom.NextDouble().ToString(); // TODO: Acquire real key
                IPAddress address;
                if (!IPAddress.TryParse(LastIP, out address))
                {
                    IPHostEntry entry = Dns.GetHostEntry(LastIP);
                    if (entry.AddressList.Length == 0)
                    {
                        throw new Exception("Empty address list for DNS server at '" + LastIP + "'");
                    }
                    if (TheClient.CVars.n_first.Value.ToLower() == "ipv4")
                    {
                        foreach (IPAddress saddress in entry.AddressList)
                        {
                            if (saddress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                address = saddress;
                                break;
                            }
                        }
                    }
                    else if (TheClient.CVars.n_first.Value.ToLower() == "ipv6")
                    {
                        foreach (IPAddress saddress in entry.AddressList)
                        {
                            if (saddress.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                address = saddress;
                                break;
                            }
                        }
                    }
                    if (address == null)
                    {
                        foreach (IPAddress saddress in entry.AddressList)
                        {
                            if (saddress.AddressFamily == AddressFamily.InterNetworkV6 || saddress.AddressFamily == AddressFamily.InterNetwork)
                            {
                                address = saddress;
                                break;
                            }
                        }
                    }
                    if (address == null)
                    {
                        throw new Exception("DNS has entries, but none are IPv4 or IPv6!");
                    }
                }
                ConnectionSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ConnectionSocket.LingerState.LingerTime = 5;
                ConnectionSocket.LingerState.Enabled = true;
                ConnectionSocket.ReceiveTimeout = 10000;
                ConnectionSocket.SendTimeout = 10000;
                ConnectionSocket.ReceiveBufferSize = 5 * 1024 * 1024;
                ConnectionSocket.SendBufferSize = 5 * 1024 * 1024;
                int tport = Utilities.StringToInt(LastPort);
                ConnectionSocket.Connect(new IPEndPoint(address, tport));
                ConnectionSocket.Send(FileHandler.encoding.GetBytes("SOG__\r" + TheClient.Username
                    + "\r" + key + "\r" + LastIP + "\r" + LastPort + "\n"));
                byte[] resp = ReceiveUntil(ConnectionSocket, 50, (byte)'\n');
                if (FileHandler.encoding.GetString(resp) != "ACCEPT")
                {
                    ConnectionSocket.Close();
                    throw new Exception("Server did not accept connection");
                }
                ConnectionSocket.Blocking = false;
                SysConsole.Output(OutputType.INFO, "Connected to " + address.ToString() + ":" + tport);
                IsAlive = true;
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw ex;
                }
                SysConsole.Output(OutputType.ERROR, "Networking / connect internal: " + ex.ToString());
                ConnectionSocket.Close(5);
            }
        }

        byte[] ReceiveUntil(Socket s, int max_bytecount, byte ender)
        {
            byte[] bytes = new byte[max_bytecount];
            int gotten = 0;
            while (gotten < max_bytecount)
            {
                while (s.Available <= 0)
                {
                    Thread.Sleep(1);
                }
                s.Receive(bytes, gotten, 1, SocketFlags.None);
                if (bytes[gotten] == ender)
                {
                    byte[] got = new byte[gotten];
                    Array.Copy(bytes, got, gotten);
                    return got;
                }
                gotten++;
            }
            throw new Exception("Maximum byte count reached without valid ender");
        }

    }
}
