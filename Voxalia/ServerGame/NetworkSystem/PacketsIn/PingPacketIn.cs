using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class PingPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1)
            {
                SysConsole.Output(OutputType.WARNING, "Ping length != 1");
                return false;
            }
            byte expect = (Chunk ? Player.LastCPingByte: Player.LastPingByte);
            if (data[0] != expect)
            {
                SysConsole.Output(OutputType.WARNING, "Chunk=" + Chunk + ", d0 bad, expecting " + (int)expect + ", got " + data[0]);
                return false;
            }
            if (Chunk)
            {
                Player.LastCPingByte = (byte)Utilities.UtilRandom.Next(1, 255);
                Player.ChunkNetwork.SendPacket(new PingPacketOut(Player.LastCPingByte));
            }
            else
            {
                Player.LastPingByte = (byte)Utilities.UtilRandom.Next(1, 255);
                Player.Network.SendPacket(new PingPacketOut(Player.LastPingByte));
            }
            return true;
        }
    }
}
