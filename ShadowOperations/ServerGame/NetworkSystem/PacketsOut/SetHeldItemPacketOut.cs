using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class SetHeldItemPacketOut: AbstractPacketOut
    {
        public SetHeldItemPacketOut(int it)
        {
            ID = 23;
            Data = Utilities.IntToBytes(it);
        }
    }
}
