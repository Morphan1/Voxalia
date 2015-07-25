using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class TeleportPacketOut : AbstractPacketOut
    {
        public TeleportPacketOut(Location pos)
        {
            ID = 27;
            Data = pos.ToBytes();
        }
    }
}
