using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SetHeldItemPacketOut: AbstractPacketOut
    {
        public SetHeldItemPacketOut(int it)
        {
            ID = ServerToClientPacket.SET_HELD_ITEM;
            Data = Utilities.IntToBytes(it);
        }
    }
}
