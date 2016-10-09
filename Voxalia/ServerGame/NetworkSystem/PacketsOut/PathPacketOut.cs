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
    public class PathPacketOut: AbstractPacketOut
    {
        public PathPacketOut(List<Location> locs)
        {
            UsageType = NetUsageType.EFFECTS;
            ID = ServerToClientPacket.PATH;
            Data = new byte[locs.Count * 24 + 4];
            Utilities.IntToBytes(locs.Count).CopyTo(Data, 0);
            for (int i = 0; i < locs.Count; i++)
            {
                locs[i].ToDoubleBytes().CopyTo(Data, 4 + 24 * i);
            }
        }
    }
}
