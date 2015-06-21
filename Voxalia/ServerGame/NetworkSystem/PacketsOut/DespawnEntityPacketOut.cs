using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
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
