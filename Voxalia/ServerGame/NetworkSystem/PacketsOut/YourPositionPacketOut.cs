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
            pos.ToDoubleBytes().CopyTo(Data, 8);
            vel.ToDoubleBytes().CopyTo(Data, 8 + 24);
            Data[8 + 24 + 24] = (byte)((stance == Stance.Standing ? 0 : 1) | (pup ? 2: 0));
            Utilities.DoubleToBytes(delta).CopyTo(Data, 8 + 24 + 24 + 1);
        }
    }
}
