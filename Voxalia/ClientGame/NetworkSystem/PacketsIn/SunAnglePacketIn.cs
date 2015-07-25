using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SunAnglePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 4)
            {
                return false;
            }
            float yaw = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            float pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4, 4));
            TheClient.SunAngle.Yaw = yaw;
            TheClient.SunAngle.Pitch = pitch;
            return true;
        }
    }
}
