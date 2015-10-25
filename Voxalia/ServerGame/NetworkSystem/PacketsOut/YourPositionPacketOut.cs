using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos, Location vel, Location avel, Stance stance)
        {
            ID = 1;
            Data = new byte[12 + 12 + 1];
            pos.ToBytes().CopyTo(Data, 0);
            vel.ToBytes().CopyTo(Data, 12);
            Data[12 + 12] = (byte)(stance == Stance.Standing ? 0 : 1);
        }
    }
}
