//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class MessagePacketOut: AbstractPacketOut
    {
        public MessagePacketOut(TextChannel chan, string msg)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.MESSAGE;
            byte[] text = FileHandler.encoding.GetBytes(msg);
            Data = new byte[1 + text.Length];
            Data[0] = (byte)chan;
            text.CopyTo(Data, 1);
        }
    }
}
