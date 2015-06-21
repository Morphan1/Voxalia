using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class NetStringPacketOut: AbstractPacketOut
    {
        public NetStringPacketOut(string str)
        {
            ID = 9;
            Data = FileHandler.encoding.GetBytes(str);
        }
    }
}
