using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class YourPositionPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 1)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location vel = Location.FromBytes(data, 12);
            Location dir = pos - TheClient.Player.GetPosition();
            TheClient.Player.ServerLocation = pos;
            if (dir.LengthSquared() < TheClient.CVars.n_movement_maxdistance.ValueF * TheClient.CVars.n_movement_maxdistance.ValueF)
            {
                TheClient.Player.SetPosition(TheClient.Player.GetPosition() + dir / (TheClient.CVars.n_movement_adjustment.ValueF * TheClient.Delta));
                Location veldir = vel - TheClient.Player.GetVelocity();
                TheClient.Player.SetVelocity(TheClient.Player.GetVelocity() + veldir / (TheClient.CVars.n_movement_adjustment.ValueF * TheClient.Delta));
            }
            else
            {
                TheClient.Player.SetPosition(pos);
                TheClient.Player.SetVelocity(vel);
            }
            TheClient.Player.CBody.StanceManager.DesiredStance = data[12 + 12] == 0 ? Stance.Standing : Stance.Crouching;
            return true;
        }
    }
}
