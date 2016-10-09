//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class DefaultSoundPacketOut: AbstractPacketOut
    {
        public DefaultSoundPacketOut(Location loc, DefaultSound sound, byte subdat)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.DEFAULT_SOUND;
            Data = new byte[24 + 1 + 1];
            loc.ToDoubleBytes().CopyTo(Data, 0);
            Data[24] = (byte)sound;
            Data[24 + 1] = subdat;
        }
    }
}
