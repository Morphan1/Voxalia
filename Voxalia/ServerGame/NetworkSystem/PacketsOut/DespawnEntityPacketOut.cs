using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class DespawnEntityPacketOut: AbstractPacketOut
    {
        public DespawnEntityPacketOut(long EID)
        {
            ID = 8;
            Data = Utilities.LongToBytes(EID);
        }
    }
}
