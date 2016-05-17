﻿using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class OperationStatusPacketOut: AbstractPacketOut
    {
        public OperationStatusPacketOut(StatusOperation operation, byte status)
        {
            ID = ServerToClientPacket.OPERATION_STATUS;
            Data = new byte[] { (byte)operation, status };
        }
    }
}
