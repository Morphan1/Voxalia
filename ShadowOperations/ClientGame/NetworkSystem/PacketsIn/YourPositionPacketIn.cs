using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class YourPositionPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 12)
            {
                return false;
            }
            Location pos = Location.FromBytes(data, 0);
            Location dir = pos - TheClient.Player.GetPosition();
            TheClient.Player.SetPosition(TheClient.Player.GetPosition() + dir / 15f); // TODO: Replace '15f' with a CVar
            Location vel = Location.FromBytes(data, 12);
            Location veldir = vel - TheClient.Player.GetVelocity();
            TheClient.Player.SetVelocity(TheClient.Player.GetVelocity() + veldir / 15f); // TODO: Replace '15f' with a CVar
            return true;
        }
    }
}
