using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsOut
{
    public class HoldItemPacketOut: AbstractPacketOut
    {
        public HoldItemPacketOut(int item)
        {
            ID = 3;
            Data = Utilities.IntToBytes(item);
        }
    }
}
