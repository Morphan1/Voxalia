//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class YourStatusPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4 + 4 + 1)
            {
                return false;
            }
            float health = Utilities.BytesToFloat(Utilities.BytesPartial(data, 0, 4));
            float maxhealth = Utilities.BytesToFloat(Utilities.BytesPartial(data, 4, 4));
            TheClient.Player.Health = health;
            TheClient.Player.MaxHealth = maxhealth;
            TheClient.Player.ServerFlags = (YourStatusFlags)data[4 + 4];
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.NON_SOLID))
            {
                TheClient.Player.Desolidify();
            }
            else
            {
                TheClient.Player.Solidify();
            }
            return true;
        }
    }
}
