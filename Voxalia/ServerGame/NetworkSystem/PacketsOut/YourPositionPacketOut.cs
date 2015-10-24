using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos, Location vel, Location avel, PlayerStance stance)
        {
            ID = 1;
            Data = new byte[12 + 12];
            pos.ToBytes().CopyTo(Data, 0);
            vel.ToBytes().CopyTo(Data, 12);
        }
    }
}
