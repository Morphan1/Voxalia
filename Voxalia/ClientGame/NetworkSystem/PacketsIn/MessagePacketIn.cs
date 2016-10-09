//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.ClientGame.UISystem;
using Voxalia.Shared.Files;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class MessagePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 1)
            {
                return false;
            }
            TextChannel tc = (TextChannel)data[0];
            TheClient.WriteMessage(tc, FileHandler.encoding.GetString(data, 1, data.Length - 1));
            return true;
        }
    }
}
