using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(long tID, KeysPacketData data, Location direction)
        {
            ID = 1;
            Data = new byte[8 + 2 + 4 + 4];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            Utilities.UshortToBytes((ushort)data).CopyTo(Data, 8);
            Utilities.FloatToBytes((float)direction.Yaw).CopyTo(Data, 8 + 2);
            Utilities.FloatToBytes((float)direction.Pitch).CopyTo(Data, 8 + 2 + 4);
        }
    }

    public enum KeysPacketData: ushort
    {
        FORWARD = 1,
        BACKWARD = 2,
        LEFTWARD = 4,
        RIGHTWARD = 8,
        UPWARD = 16,
        WALK = 32,
        CLICK = 64,
        ALTCLICK = 128,
        SPRINT = 256,
        DOWNWARD = 256 * 2,
        USE = 256 * 4
    }
}
