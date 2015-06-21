using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
