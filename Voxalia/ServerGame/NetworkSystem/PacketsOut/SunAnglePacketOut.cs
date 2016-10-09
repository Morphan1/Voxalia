//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class SunAnglePacketOut: AbstractPacketOut
    {
        public SunAnglePacketOut(double yaw, double pitch)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.SUN_ANGLE;
            Data = new byte[4 + 4];
            Utilities.FloatToBytes((float)yaw).CopyTo(Data, 0);
            Utilities.FloatToBytes((float)pitch).CopyTo(Data, 4);
        }
    }
}
