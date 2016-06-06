using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(long tID, KeysPacketData data, Location direction, float xmove, float ymove, Location pos, Location vel)
        {
            ID = ClientToServerPacket.KEYS;
            Data = new byte[8 + 2 + 4 + 4 + 4 + 4 + 12 + 12];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            Utilities.UshortToBytes((ushort)data).CopyTo(Data, 8);
            Utilities.FloatToBytes((float)direction.Yaw).CopyTo(Data, 8 + 2);
            Utilities.FloatToBytes((float)direction.Pitch).CopyTo(Data, 8 + 2 + 4);
            Utilities.FloatToBytes(xmove).CopyTo(Data, 8 + 2 + 4 + 4);
            Utilities.FloatToBytes(ymove).CopyTo(Data, 8 + 2 + 4 + 4 + 4);
            int s = 8 + 2 + 4 + 4 + 4 + 4;
            pos.ToBytes().CopyTo(Data, s);
            vel.ToBytes().CopyTo(Data, s + 12);
        }
    }
}
