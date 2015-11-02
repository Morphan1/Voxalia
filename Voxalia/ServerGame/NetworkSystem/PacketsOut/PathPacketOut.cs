using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PathPacketOut: AbstractPacketOut
    {
        public PathPacketOut(List<Location> locs)
        {
            ID = 30;
            Data = new byte[locs.Count * 12 + 4];
            Utilities.IntToBytes(locs.Count).CopyTo(Data, 0);
            for (int i = 0; i < locs.Count; i++)
            {
                locs[i].ToBytes().CopyTo(Data, 4 + 12 * i);
            }
        }
    }
}
