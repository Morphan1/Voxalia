//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SunAnglePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 4)
            {
                return false;
            }
            float yaw = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            float pitch = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4, 4));
            TheClient.SunAngle.Yaw = yaw;
            TheClient.SunAngle.Pitch = pitch;
            return true;
        }
    }
}
