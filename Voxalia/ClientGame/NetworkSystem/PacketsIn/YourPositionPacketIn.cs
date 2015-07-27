using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class YourPositionPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12 + 12 + 1)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location vel = Location.FromBytes(data, 12);
            Location avel = Location.FromBytes(data, 12 + 12);
            Location dir = pos - TheClient.Player.GetPosition();
            TheClient.Player.ServerLocation = pos;
            if (dir.LengthSquared() < 20 * 20) // TODO: replace '20' with a CVar
            {
                TheClient.Player.SetPosition(TheClient.Player.GetPosition() + dir / 60f); // TODO: Replace '120f' with a CVar * PacketDelta?
                Location veldir = vel - TheClient.Player.GetVelocity();
                TheClient.Player.SetVelocity(TheClient.Player.GetVelocity() + veldir / 60f); // TODO: Replace '120f' with a CVar * PacketDelta?
                Location aveldir = avel - Location.FromBVector(TheClient.Player.WheelBody.AngularVelocity);
                TheClient.Player.WheelBody.AngularVelocity = TheClient.Player.WheelBody.AngularVelocity + (avel / 60f).ToBVector(); // TODO: Replace '120f' with a CVar * PacketDelta?
            }
            else
            {
                TheClient.Player.SetPosition(pos);
                TheClient.Player.SetVelocity(vel);
                TheClient.Player.WheelBody.AngularVelocity = avel.ToBVector();
            }
            byte st = data[12 + 12 + 12];
            PlayerStance stance = PlayerStance.STAND;
            if (st == 2)
            {
                stance = PlayerStance.CRAWL;
            }
            else if (st == 1)
            {
                stance = PlayerStance.CROUCH;
            }
            TheClient.Player.Stance = stance;
            return true;
        }
    }
}
