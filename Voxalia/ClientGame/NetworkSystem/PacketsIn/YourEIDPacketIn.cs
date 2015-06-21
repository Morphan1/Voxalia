using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class YourEIDPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8)
            {
                return false;
            }
            TheClient.Player.EID = Utilities.BytesToLong(data);
            return true;
        }
    }
}
