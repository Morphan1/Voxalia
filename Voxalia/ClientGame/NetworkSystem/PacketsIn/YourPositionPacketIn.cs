using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.EntitySystem;

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
            if (dir.LengthSquared() < 20 * 20) // TODO: replace '20' with a CVar
            {
                TheClient.Player.SetPosition(TheClient.Player.GetPosition() + dir / 120f); // TODO: Replace '120f' with a CVar * PacketDelta?
                Location veldir = vel - TheClient.Player.GetVelocity();
                TheClient.Player.SetVelocity(TheClient.Player.GetVelocity() + veldir / 120f); // TODO: Replace '120f' with a CVar * PacketDelta?
            }
            else
            {
                TheClient.Player.SetPosition(pos);
                TheClient.Player.SetVelocity(vel);
            }
            byte st = data[12 + 12];
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
