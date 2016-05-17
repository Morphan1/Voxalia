using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class TeleportPacketOut : AbstractPacketOut
    {
        public TeleportPacketOut(Location pos)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.TELEPORT;
            Data = pos.ToBytes();
        }
    }
}
