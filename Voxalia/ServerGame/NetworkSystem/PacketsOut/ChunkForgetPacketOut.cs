using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ChunkForgetPacketOut: AbstractPacketOut
    {
        public ChunkForgetPacketOut(Vector3i cpos)
        {
            UsageType = NetUsageType.CHUNKS;
            ID = ServerToClientPacket.CHUNK_FORGET;
            Data = new byte[12];
            Utilities.IntToBytes(cpos.X).CopyTo(Data, 0);
            Utilities.IntToBytes(cpos.Y).CopyTo(Data, 4);
            Utilities.IntToBytes(cpos.Z).CopyTo(Data, 8);
        }
    }
}
