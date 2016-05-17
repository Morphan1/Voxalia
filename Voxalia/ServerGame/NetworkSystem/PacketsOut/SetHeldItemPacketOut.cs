using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SetHeldItemPacketOut: AbstractPacketOut
    {
        public SetHeldItemPacketOut(int it)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.SET_HELD_ITEM;
            Data = Utilities.IntToBytes(it);
        }
    }
}
