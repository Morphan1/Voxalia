//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class DisconnectPacketOut: AbstractPacketOut
    {
        public DisconnectPacketOut()
        {
            ID = ClientToServerPacket.DISCONNECT;
            Data = new byte[] { 0 };
        }
    }
}
