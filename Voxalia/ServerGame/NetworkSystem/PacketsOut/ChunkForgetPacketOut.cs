using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ChunkForgetPacketOut: AbstractPacketOut
    {
        public ChunkForgetPacketOut(Location cpos)
        {
            ID = 31;
            Data = new byte[12];
            Utilities.IntToBytes((int)cpos.X).CopyTo(Data, 0);
            Utilities.IntToBytes((int)cpos.Y).CopyTo(Data, 4);
            Utilities.IntToBytes((int)cpos.Z).CopyTo(Data, 8);
        }
    }
}
