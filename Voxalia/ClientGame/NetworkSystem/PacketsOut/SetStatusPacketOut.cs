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

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class SetStatusPacketOut: AbstractPacketOut
    {
        public SetStatusPacketOut(ClientStatus opt, byte status)
        {
            ID = ClientToServerPacket.SET_STATUS;
            Data = new byte[2];
            Data[0] = (byte)opt;
            Data[1] = status;
        }
    }
}
