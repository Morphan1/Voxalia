using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.OtherSystems;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class SetItemPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4 + 4)
            {
                return false;
            }
            int spot = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            if (spot < 0 || spot > TheClient.Items.Count)
            {
                return false;
            }
            byte[] dat = Utilities.BytesPartial(data, 4, data.Length - 4);
            try
            {
                ItemStack item = new ItemStack(TheClient, dat);
                TheClient.Items[spot] = item;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        } 
    }
}
