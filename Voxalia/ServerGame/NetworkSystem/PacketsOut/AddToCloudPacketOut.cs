using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class AddToCloudPacketOut: AbstractPacketOut
    {
        public AddToCloudPacketOut(Cloud c, int s)
        {
            UsageType = NetUsageType.CLOUDS;
            ID = ServerToClientPacket.ADD_TO_CLOUD;
            Data = new byte[24 + 4 + 4 + 8];
            c.Points[s].ToDoubleBytes().CopyTo(Data, 0);
            Utilities.FloatToBytes((float)c.Sizes[s]).CopyTo(Data, 24);
            Utilities.FloatToBytes((float)c.EndSizes[s]).CopyTo(Data, 24 + 4);
            Utilities.LongToBytes(c.CID).CopyTo(Data, 24 + 4 + 4);
        }
    }
}
