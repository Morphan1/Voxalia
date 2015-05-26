using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class RemoveItemPacketOut: AbstractPacketOut
    {
        public RemoveItemPacketOut(int spot)
        {
            ID = 19;
            Data = Utilities.IntToBytes(spot);
        }
    }
}
