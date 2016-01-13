using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class AddToCloudPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12 + 4 + 4 + 8)
            {
                return false;
            }
            Location loc = Location.FromBytes(data, 0);
            float size = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12, 4));
            float endsize = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 4, 4));
            long CID = Utilities.BytesToLong(Utilities.BytesPartial(data, 12 + 4 + 4, 8));
            for (int i = 0; i < TheClient.TheRegion.Clouds.Count; i++)
            {
                if (TheClient.TheRegion.Clouds[i].CID == CID)
                {
                    TheClient.TheRegion.Clouds[i].Points.Add(loc);
                    TheClient.TheRegion.Clouds[i].Sizes.Add(size);
                    TheClient.TheRegion.Clouds[i].EndSizes.Add(endsize);
                    return true;
                }
            }
            return false;
        }
    }
}
