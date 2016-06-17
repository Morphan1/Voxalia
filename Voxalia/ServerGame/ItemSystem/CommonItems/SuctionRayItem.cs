using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class SuctionRayItem : BaseForceRayItem
    {
        public SuctionRayItem()
        {
            Name = "suctionray";
        }

        public override float GetStrength()
        {
            return 1;
        }
    }
}
