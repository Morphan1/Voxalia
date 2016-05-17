using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class HighlightPacketOut : AbstractPacketOut
    {
        public HighlightPacketOut(params AABB[] sels)
        {
            UsageType = NetUsageType.EFFECTS;
            int c = 0;
            for (int i = 0; i < sels.Length; i++)
            {
                if (!sels[i].Min.IsNaN())
                {
                    c++;
                }
            }
            ID = ServerToClientPacket.HIGHLIGHT;
            Data = new byte[4 + 12 * 2 * c];
            Utilities.IntToBytes(c).CopyTo(Data, 0);
            int t = 0;
            for (int i = 0; i < sels.Length; i++)
            {
                if (!sels[i].Min.IsNaN())
                {
                    sels[i].Min.ToBytes().CopyTo(Data, 4 + t * 12 * 2);
                    sels[i].Max.ToBytes().CopyTo(Data, 4 + t * 12 * 2 + 12);
                    t++;
                }
            }
        }
    }
}
