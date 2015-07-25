using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class FlashLightPacketOut: AbstractPacketOut
    {
        public FlashLightPacketOut(PlayerEntity player, bool enabled, float distance, Location color)
        {
            ID = 18;
            Data = new byte[8 + 1 + 4 + 12];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            Data[8] = (byte)(enabled ? 1 : 0);
            Utilities.FloatToBytes(distance).CopyTo(Data, 8 + 1);
            color.ToBytes().CopyTo(Data, 8 + 1 + 4);
        }
    }
}
