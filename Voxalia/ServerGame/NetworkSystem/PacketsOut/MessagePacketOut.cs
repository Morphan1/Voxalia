using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    class MessagePacketOut: AbstractPacketOut
    {
        public MessagePacketOut(string msg)
        {
            ID = 5;
            Data = FileHandler.encoding.GetBytes(msg);
        }
    }
}
