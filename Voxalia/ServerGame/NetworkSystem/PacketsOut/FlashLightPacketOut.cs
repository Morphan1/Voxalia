using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class FlashLightPacketOut: AbstractPacketOut
    {
        public FlashLightPacketOut(PlayerEntity player, bool enabled, double distance, Location color)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.FLASHLIGHT;
            Data = new byte[8 + 1 + 4 + 24];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            Data[8] = (byte)(enabled ? 1 : 0);
            Utilities.FloatToBytes((float)distance).CopyTo(Data, 8 + 1);
            color.ToDoubleBytes().CopyTo(Data, 8 + 1 + 4);
        }
    }
}
