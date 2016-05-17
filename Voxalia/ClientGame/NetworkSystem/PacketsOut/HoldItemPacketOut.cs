using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class HoldItemPacketOut: AbstractPacketOut
    {
        public HoldItemPacketOut(int item)
        {
            ID = ClientToServerPacket.HOLD_ITEM;
            Data = Utilities.IntToBytes(item);
        }
    }
}
