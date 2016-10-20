//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class SpawnItemPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4 + 4)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid packet len: " + data.Length);
                return false;
            }
            int spot = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            if (spot < 0 || spot > TheClient.Items.Count)
            {
                SysConsole.Output(OutputType.WARNING, "Bad spot:" + spot);
                return false;
            }
            byte[] dat = Utilities.BytesPartial(data, 4, data.Length - 4);
            try
            {
                ItemStack item = new ItemStack(TheClient, dat);
                TheClient.Items.Insert(spot, item);
                //TheClient.UpdateInventoryMenu();
                return true;
            }
            catch (Exception ex)
            {
                SysConsole.Output("Spawning item", ex);
                return false;
            }
        }
    }
}
