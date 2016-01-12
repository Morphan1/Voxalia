using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class RemoveCloudPacketOut: AbstractPacketOut
    {
        public RemoveCloudPacketOut(long cid)
        {
            ID = 36;
            Data = Utilities.LongToBytes(cid);
        }
    }
}
