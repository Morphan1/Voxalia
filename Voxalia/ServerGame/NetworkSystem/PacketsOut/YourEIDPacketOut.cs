using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class YourEIDPacketOut : AbstractPacketOut
    {
        public YourEIDPacketOut(long EID)
        {
            ID = 13;
            Data = Utilities.LongToBytes(EID);
        }
    }
}
