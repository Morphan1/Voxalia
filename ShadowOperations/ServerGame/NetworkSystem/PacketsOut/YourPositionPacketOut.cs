using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos, Location vel)
        {
            ID = 1;
            Data = new byte[12 + 12];
            pos.ToBytes().CopyTo(Data, 0);
            vel.ToBytes().CopyTo(Data, 12);
        }
    }
}
