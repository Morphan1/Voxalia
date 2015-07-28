using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Files;
using System.Threading.Tasks;

namespace Voxalia.ClientGame.NetworkSystem
{
    public class NetworkBase
    {
        public Client TheClient;

        public NetworkBase(Client tclient)
        {
            TheClient = tclient;
            Strings = new NetStringManager();
            recd = new byte[MAX];
            recd2 = new byte[MAX];
        }

        public NetStringManager Strings;

        public Socket ConnectionSocket;

        public Socket ChunkSocket;

        public Thread ConnectionThread;

        public string LastIP;

        public string LastPort;

        public bool IsAlive = false;

        bool norep = false;

        public void Disconnect()
        {
            if (norep)
            {
                return;
            }
            norep = true;
            if (ConnectionThread != null && ConnectionThread.IsAlive)
            {
                try
                {
                    ConnectionThread.Abort();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.WARNING, "Disconnecting: " + ex.ToString());
                }
                ConnectionThread = null;
            }
            if (ConnectionSocket != null)
            {
                if (IsAlive)
                {
                    try
                    {
                        SendPacket(new DisconnectPacketOut());
                    }
                    catch (Exception ex)
                    {
                        SysConsole.Output(OutputType.WARNING, "Disconnecting: " + ex.ToString());
                    }
                }
                Socket csock = ConnectionSocket;
                TheClient.Schedule.ScheduleSyncTask(() => csock.Close(2), 2);
                ConnectionSocket = null;
            }
            if (ChunkSocket != null)
            {
                Socket csock = ChunkSocket;
                TheClient.Schedule.ScheduleSyncTask(() => csock.Close(2), 2);
                ChunkSocket = null;
            }
            IsAlive = false;
            norep = false;
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

        byte[] recd2;

        int recdsofar2 = 0;

        bool pLive = false;

        public void TickSocket(Socket sock, ref byte[] rd, ref int rdsf)
        {
            int avail = sock.Available;
            if (avail <= 0)
            {
                return;
            }
            if (avail + rdsf > MAX)
            {
                throw new Exception("Received too much data!");
            }
            sock.Receive(rd, rdsf, avail, SocketFlags.None);
            rdsf += avail;
            if (rdsf < 5)
            {
                return;
            }
            while (true)
            {
                byte[] len_bytes = new byte[4];
                Array.Copy(rd, len_bytes, 4);
                int len = Utilities.BytesToInt(len_bytes);
                if (len + 5 > MAX)
                {
                    throw new Exception("Unreasonably huge packet!");
                }
                if (rdsf < 5 + len)
                {
                    return;
                }
                byte packetID = rd[4];
                byte[] data = new byte[len];
                Array.Copy(rd, 5, data, 0, len);
                byte[] rem_data = new byte[rdsf - (len + 5)];
                if (rem_data.Length > 0)
                {
                    Array.Copy(rd, len + 5, rem_data, 0, rem_data.Length);
                    Array.Copy(rem_data, rd, rem_data.Length);
                }
                rdsf -= len + 5;
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
                    case 20:
                        // packet = new JointStatusPacketIn();
                        throw new NotImplementedException();
                    case 21:
                        packet = new SetItemPacketIn();
                        break;
                    case 22:
                        packet = new CVarSetPacketIn();
                        break;
                    case 23:
                        packet = new SetHeldItemPacketIn();
                        break;
                    case 24:
                        packet = new ChunkInfoPacketIn();
                        break;
                    case 25:
                        packet = new BlockEditPacketIn();
                        break;
                    case 26:
                        packet = new SunAnglePacketIn();
                        break;
                    case 27:
                        packet = new TeleportPacketIn();
                        break;
                    case 28:
                        packet = new OperationStatusPacketIn();
                        break;
                    default:
                        throw new Exception("Invalid packet ID: " + packetID);
                }
                packet.TheClient = TheClient;
                packet.ChunkN = sock == ChunkSocket;
                TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    if (!packet.ParseBytesAndExecute(data))
                    {
                        SysConsole.Output(OutputType.ERROR, "Bad packet (ID=" + packetID + ") data!");
                    }
                });
            }
        }

        public void LaunchTicker()
        {
            TheClient.Schedule.StartASyncTask(() =>
            {
                while (true)
                {
                    try
                    {
                        if (!Tick())
                        {
                            return;
                        }
                        Thread.Sleep(16);
                    }
                    catch (Exception ex)
                    {
                        SysConsole.Output(OutputType.ERROR, "Connection: " + ex.ToString());
                    }
                }
            });
        }

        bool Tick()
        {
            // TODO: Connection timeout
            if (!IsAlive)
            {
                if (pLive)
                {
                    TheClient.Schedule.ScheduleSyncTask(() => { TheClient.ShowMainMenu(); });
                    pLive = false;
                }
                return false;
            }
            try
            {
                if (!pLive)
                {
                    TheClient.Schedule.ScheduleSyncTask(() => { TheClient.ShowChunkWaiting(); });
                    pLive = true;
                }
                TickSocket(ConnectionSocket, ref recd, ref recdsofar);
                TickSocket(ChunkSocket, ref recd2, ref recdsofar2);
                return true;
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Forcibly disconnected from server: " + ex.GetType().Name + ": " + ex.Message);
                SysConsole.Output(OutputType.INFO, ex.ToString()); // TODO: Make me 'debug only'!
                Disconnect();
                return false;
            }
        }

        public byte[] GetBytesFor(AbstractPacketOut packet)
        {
            byte id = packet.ID;
            byte[] data = packet.Data;
            byte[] fdata = new byte[data.Length + 5];
            Utilities.IntToBytes(data.Length).CopyTo(fdata, 0);
            fdata[4] = id;
            data.CopyTo(fdata, 5);
            return fdata;
        }

        public void SendPacket(AbstractPacketOut packet)
        {
            if (!IsAlive)
            {
                return;
            }
            try
            {
                ConnectionSocket.Send(GetBytesFor(packet));
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.WARNING, "Forcibly disconnected from server: " + ex.GetType().Name + ": " + ex.Message);
                Disconnect();
            }
        }

        public void SendChunkPacket(AbstractPacketOut packet)
        {
            if (!IsAlive)
            {
                return;
            }
            try
            {
                ChunkSocket.Send(GetBytesFor(packet));
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
                ConnectionSocket.Send(FileHandler.encoding.GetBytes("VOX__\r" + TheClient.Username
                    + "\r" + key + "\r" + LastIP + "\r" + LastPort + "\n"));
                byte[] resp = ReceiveUntil(ConnectionSocket, 50, (byte)'\n');
                if (FileHandler.encoding.GetString(resp) != "ACCEPT")
                {
                    ConnectionSocket.Close();
                    throw new Exception("Server did not accept connection");
                }
                ConnectionSocket.Blocking = false;
                ChunkSocket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ChunkSocket.LingerState.LingerTime = 5;
                ChunkSocket.LingerState.Enabled = true;
                ChunkSocket.ReceiveTimeout = 10000;
                ChunkSocket.SendTimeout = 10000;
                ChunkSocket.ReceiveBufferSize = 5 * 1024 * 1024;
                ChunkSocket.SendBufferSize = 5 * 1024 * 1024;
                ChunkSocket.Connect(new IPEndPoint(address, tport));
                ChunkSocket.Send(FileHandler.encoding.GetBytes("VOXc_\r" + TheClient.Username
                    + "\r" + key + "\r" + LastIP + "\r" + LastPort + "\n"));
                resp = ReceiveUntil(ChunkSocket, 50, (byte)'\n');
                if (FileHandler.encoding.GetString(resp) != "ACCEPT")
                {
                    ConnectionSocket.Close();
                    ChunkSocket.Close();
                    throw new Exception("Server did not accept connection");
                }
                ChunkSocket.Blocking = false;
                SysConsole.Output(OutputType.INFO, "Connected to " + address.ToString() + ":" + tport);
                IsAlive = true;
                LaunchTicker();
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw ex;
                }
                SysConsole.Output(OutputType.ERROR, "Networking / connect internal: " + ex.ToString());
                if (ConnectionSocket != null)
                {
                    ConnectionSocket.Close(5);
                }
                if (ChunkSocket != null)
                {
                    ChunkSocket.Close(5);
                }
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
