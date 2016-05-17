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
            ID = ServerToClientPacket.REMOVE_CLOUD;
            Data = Utilities.LongToBytes(cid);
        }
    }
}
