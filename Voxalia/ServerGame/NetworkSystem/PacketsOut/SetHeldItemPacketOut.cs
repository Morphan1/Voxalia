using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
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
