using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(long tID, KeysPacketData data, Location direction, float xmove, float ymove)
        {
            ID = 1;
            Data = new byte[8 + 2 + 4 + 4 + 4 + 4];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            Utilities.UshortToBytes((ushort)data).CopyTo(Data, 8);
            Utilities.FloatToBytes((float)direction.Yaw).CopyTo(Data, 8 + 2);
            Utilities.FloatToBytes((float)direction.Pitch).CopyTo(Data, 8 + 2 + 4);
            Utilities.FloatToBytes(xmove).CopyTo(Data, 8 + 2 + 4 + 4);
            Utilities.FloatToBytes(ymove).CopyTo(Data, 8 + 2 + 4 + 4 + 4);
        }
    }

    public enum KeysPacketData: ushort
    {
        UPWARD = 1,
        CLICK = 2,
        ALTCLICK = 4,
        DOWNWARD = 8,
        USE = 16
    }
}
