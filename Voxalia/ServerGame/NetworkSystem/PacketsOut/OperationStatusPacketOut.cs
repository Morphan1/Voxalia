using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class OperationStatusPacketOut: AbstractPacketOut
    {
        public OperationStatusPacketOut(StatusOperation operation, byte status)
        {
            ID = 28;
            Data = new byte[] { (byte)operation, status };
        }
    }
}
