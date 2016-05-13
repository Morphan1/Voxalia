using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class HighlightPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length < 4)
            {
                return false;
            }
            int len = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            if (data.Length != 4 + 12 * 2 * len)
            {
                return false;
            }
            AABB[] boxes = new AABB[len];
            for (int i = 0; i < len; i++)
            {
                boxes[i] = new AABB();
                boxes[i].Min = Location.FromBytes(data, 4 + i * 12 * 2);
                boxes[i].Max = Location.FromBytes(data, 4 + i * 12 * 2 + 12);
            }
            TheClient.TheRegion.Highlights = boxes;
            return true;
        }
    }
}
