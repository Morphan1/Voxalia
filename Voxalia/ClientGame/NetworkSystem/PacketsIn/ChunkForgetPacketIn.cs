using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class ChunkForgetPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12)
            {
                return false;
            }
            int x = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            int y = Utilities.BytesToInt(Utilities.BytesPartial(data, 4, 4));
            int z = Utilities.BytesToInt(Utilities.BytesPartial(data, 8, 4));
            TheClient.TheRegion.ForgetChunk(new Location(x, y, z));
            return true;
        }
    }
}
