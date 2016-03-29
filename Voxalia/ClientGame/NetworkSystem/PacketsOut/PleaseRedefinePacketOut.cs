using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    class PleaseRedefinePacketOut : AbstractPacketOut
    {
        public PleaseRedefinePacketOut(long eid)
        {
            ID = 6;
            Data = Utilities.LongToBytes(eid);
        }
    }
}
