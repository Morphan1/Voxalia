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
    public class SetStatusPacketOut: AbstractPacketOut
    {
        public SetStatusPacketOut(PlayerEntity player, ClientStatus opt, byte status)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.SET_STATUS;
            Data = new byte[8 + 1 + 1];
            Utilities.LongToBytes(player.EID).CopyTo(Data, 0);
            Data[8] = (byte)opt;
            Data[8 + 1] = status;
        }
    }
}
