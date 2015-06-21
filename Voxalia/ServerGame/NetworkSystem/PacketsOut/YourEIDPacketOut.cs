using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
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
