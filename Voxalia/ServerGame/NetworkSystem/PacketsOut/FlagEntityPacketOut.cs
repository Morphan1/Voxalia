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
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class FlagEntityPacketOut: AbstractPacketOut
    {
        public FlagEntityPacketOut(Entity e, EntityFlag flag, double value)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.FLAG_ENTITY;
            Data = new byte[8 + 1 + 4];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Data[8] = (byte)flag;
            Utilities.FloatToBytes((float)value).CopyTo(Data, 8 + 1);
        }
    }
}
