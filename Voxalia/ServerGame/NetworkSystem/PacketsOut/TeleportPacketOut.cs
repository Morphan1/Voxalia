using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class TeleportPacketOut : AbstractPacketOut
    {
        public TeleportPacketOut(Location pos)
        {
            ID = ServerToClientPacket.TELEPORT;
            Data = pos.ToBytes();
        }
    }
}
