using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourStatusPacketOut: AbstractPacketOut
    {
        public YourStatusPacketOut(double health, double max_health, YourStatusFlags flags)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.YOUR_STATUS;
            Data = new byte[4 + 4 + 1];
            Utilities.FloatToBytes((float)health).CopyTo(Data, 0);
            Utilities.FloatToBytes((float)max_health).CopyTo(Data, 4);
            Data[4 + 4] = (byte)flags;
        }
    }
}
