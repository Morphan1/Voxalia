using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using BEPUutilities;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class TeleportPacketOut : AbstractPacketOut
    {
        public TeleportPacketOut(Location pos)
        {
            ID = 27;
            Data = new byte[12];
            pos.ToBytes().CopyTo(Data, 0);
        }
    }
}
