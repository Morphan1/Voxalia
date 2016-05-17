using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(double delta, long tID, Location pos, Location vel, Location avel, Stance stance, bool pup)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.YOUR_POSITION;
            Data = new byte[8 + 12 + 12 + 1 + 8];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            pos.ToBytes().CopyTo(Data, 8);
            vel.ToBytes().CopyTo(Data, 8 + 12);
            Data[8 + 12 + 12] = (byte)((stance == Stance.Standing ? 0 : 1) | (pup ? 2: 0));
            Utilities.DoubleToBytes(delta).CopyTo(Data, 8 + 12 + 12 + 1);
        }
    }
}
