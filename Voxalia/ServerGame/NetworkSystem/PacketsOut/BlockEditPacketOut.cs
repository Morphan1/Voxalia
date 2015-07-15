using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class BlockEditPacketOut: AbstractPacketOut
    {
        public BlockEditPacketOut(Location pos, Material mat, byte dat)
        {
            ID = 25;
            Data = new byte[12 + 2 + 1];
            pos.ToBytes().CopyTo(Data, 0);
            Utilities.UshortToBytes((ushort)mat).CopyTo(Data, 12);
            Data[12 + 2] = dat;
        }
    }
}
