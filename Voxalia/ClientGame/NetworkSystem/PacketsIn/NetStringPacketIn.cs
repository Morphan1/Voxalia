//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class NetStringPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            TheClient.Network.Strings.Strings.Add(FileHandler.encoding.GetString(data));
            return true;
        }
    }
}
