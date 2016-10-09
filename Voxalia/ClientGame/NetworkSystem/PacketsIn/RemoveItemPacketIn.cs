//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class RemoveItemPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4)
            {
                return false;
            }
            if (TheClient.Items.Count == 0)
            {
                SysConsole.Output(OutputType.WARNING, "Have no items, can't remove an item!");
                return false;
            }
            int spot = Utilities.BytesToInt(data);
            while (spot < 0)
            {
                spot += TheClient.Items.Count;
            }
            while (spot >= TheClient.Items.Count)
            {
                spot -= TheClient.Items.Count;
            }
            if (spot >= 0 && spot < TheClient.Items.Count)
            {
                TheClient.Items.RemoveAt(spot);
                return true;
            }
            SysConsole.Output(OutputType.WARNING, "Got " + spot + ", expected 0 to " + TheClient.Items.Count);
            return false;
        }
    }
}
