using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos, Location vel, PlayerStance stance)
        {
            ID = 1;
            Data = new byte[12 + 12 + 1];
            pos.ToBytes().CopyTo(Data, 0);
            vel.ToBytes().CopyTo(Data, 12);
            Data[12 + 12] = (byte)(stance == PlayerStance.STAND ? 0 : (stance == PlayerStance.CROUCH ? 1 : 2));
        }
    }
}
