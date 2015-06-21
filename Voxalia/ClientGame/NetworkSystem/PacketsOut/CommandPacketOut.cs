using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsOut
{
    class CommandPacketOut: AbstractPacketOut
    {
        public CommandPacketOut(string cmd)
        {
            ID = 2;
            Data = FileHandler.encoding.GetBytes(cmd);
        }
    }
}
