//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    class CommandPacketOut: AbstractPacketOut
    {
        public CommandPacketOut(string cmd)
        {
            ID = ClientToServerPacket.COMMAND;
            Data = FileHandler.encoding.GetBytes(cmd);
        }
    }
}
