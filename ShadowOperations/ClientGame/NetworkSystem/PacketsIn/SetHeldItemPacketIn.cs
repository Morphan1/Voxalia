using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    public class SetHeldItemPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4)
            {
                return false;
            }
            int dat = Utilities.BytesToInt(data);
            TheClient.QuickBarPos = dat;
            return true;
        }
    }
}
