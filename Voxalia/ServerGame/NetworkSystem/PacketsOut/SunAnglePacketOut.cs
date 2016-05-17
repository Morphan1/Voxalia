using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SunAnglePacketOut: AbstractPacketOut
    {
        public SunAnglePacketOut(float yaw, float pitch)
        {
            ID = ServerToClientPacket.SUN_ANGLE;
            Data = new byte[4 + 4];
            Utilities.FloatToBytes(yaw).CopyTo(Data, 0);
            Utilities.FloatToBytes(pitch).CopyTo(Data, 4);
        }
    }
}
