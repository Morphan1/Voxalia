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
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class HighlightPacketOut : AbstractPacketOut
    {
        public HighlightPacketOut(params AABB[] sels)
        {
            UsageType = NetUsageType.EFFECTS;
            int c = 0;
            for (int i = 0; i < sels.Length; i++)
            {
                if (!sels[i].Min.IsNaN())
                {
                    c++;
                }
            }
            ID = ServerToClientPacket.HIGHLIGHT;
            Data = new byte[4 + 24 * 2 * c];
            Utilities.IntToBytes(c).CopyTo(Data, 0);
            int t = 0;
            for (int i = 0; i < sels.Length; i++)
            {
                if (!sels[i].Min.IsNaN())
                {
                    sels[i].Min.ToDoubleBytes().CopyTo(Data, 4 + t * 24 * 2);
                    sels[i].Max.ToDoubleBytes().CopyTo(Data, 4 + t * 24 * 2 + 24);
                    t++;
                }
            }
        }
    }
}
