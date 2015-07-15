using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class BlockEditPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 2 + 1)
            {
                return false;
            }
            Location loc = Location.FromBytes(data, 0);
            ushort mat = Utilities.BytesToUshort(Utilities.BytesPartial(data, 12, 2));
            byte dat = data[12 + 2];
            TheClient.TheWorld.SetBlockMaterial(loc, (Material)mat, dat);
            return true;
        }
    }
}
