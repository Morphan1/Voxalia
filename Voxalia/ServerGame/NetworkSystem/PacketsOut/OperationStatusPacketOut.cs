//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class OperationStatusPacketOut: AbstractPacketOut
    {
        public OperationStatusPacketOut(StatusOperation operation, byte status)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.OPERATION_STATUS;
            Data = new byte[] { (byte)operation, status };
        }
    }
}
