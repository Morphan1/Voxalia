using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class MessagePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            UIConsole.WriteLine(FileHandler.encoding.GetString(data));
            return true;
        }
    }
}
