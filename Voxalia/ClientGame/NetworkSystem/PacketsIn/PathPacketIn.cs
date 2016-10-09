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

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class PathPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            int len = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            if (data.Length < 4 + 24 * len)
            {
                return false;
            }
            Func<Location> ppos = () => TheClient.Player.GetPosition();
            for (int i = 0; i < len; i++)
            {
                Location pos = Location.FromDoubleBytes(data, 4 + 24 * i);
                TheClient.Particles.PathMark(pos, ppos);
                ppos = () => pos;
            }
            return true;
        }
    }
}
