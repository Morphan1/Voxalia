using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SunAnglePacketOut: AbstractPacketOut
    {
        public SunAnglePacketOut(double yaw, double pitch)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.SUN_ANGLE;
            Data = new byte[4 + 4];
            Utilities.FloatToBytes((float)yaw).CopyTo(Data, 0);
            Utilities.FloatToBytes((float)pitch).CopyTo(Data, 4);
        }
    }
}
