using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
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
