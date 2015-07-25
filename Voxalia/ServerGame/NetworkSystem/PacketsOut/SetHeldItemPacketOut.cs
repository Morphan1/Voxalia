using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SetHeldItemPacketOut: AbstractPacketOut
    {
        public SetHeldItemPacketOut(int it)
        {
            ID = 23;
            Data = Utilities.IntToBytes(it);
        }
    }
}
