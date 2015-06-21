using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
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
