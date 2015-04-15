using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(Location pos)
        {
            ID = 1;
            Data = pos.ToBytes();
        }
    }
}
