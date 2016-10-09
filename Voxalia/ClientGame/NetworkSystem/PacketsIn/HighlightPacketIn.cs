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

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class HighlightPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            int len = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            if (data.Length != 4 + 24 * 2 * len)
            {
                return false;
            }
            AABB[] boxes = new AABB[len];
            for (int i = 0; i < len; i++)
            {
                boxes[i] = new AABB();
                boxes[i].Min = Location.FromDoubleBytes(data, 4 + i * 24 * 2);
                boxes[i].Max = Location.FromDoubleBytes(data, 4 + i * 24 * 2 + 24);
            }
            TheClient.TheRegion.Highlights = boxes;
            return true;
        }
    }
}
