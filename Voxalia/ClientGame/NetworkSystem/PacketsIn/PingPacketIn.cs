using Voxalia.ClientGame.NetworkSystem.PacketsOut;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PingPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1)
            {
                return false;
            }
            byte bit = data[0];
            if (ChunkN)
            {
                TheClient.Network.SendChunkPacket(new PingPacketOut(bit));
            }
            else
            {
                TheClient.Network.SendPacket(new PingPacketOut(bit));
                TheClient.LastPingValue = TheClient.GlobalTickTimeLocal - TheClient.LastPingTime;
                TheClient.LastPingTime = TheClient.GlobalTickTimeLocal;
            }
            return true;
        }
    }
}
