using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos, Location vel, Location avel, PlayerStance stance)
        {
            ID = 1;
            Data = new byte[12 + 12 + 12 + 1];
            pos.ToBytes().CopyTo(Data, 0);
            vel.ToBytes().CopyTo(Data, 12);
            avel.ToBytes().CopyTo(Data, 12 + 12);
            Data[12 + 12 + 12] = (byte)(stance == PlayerStance.STAND ? 0 : (stance == PlayerStance.CROUCH ? 1 : 2));
        }
    }
}
