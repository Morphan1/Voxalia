using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class PushRayItem : BaseForceRayItem
    {
        public PushRayItem()
        {
            Name = "pushray";
        }

        public override double GetStrength()
        {
            return -1;
        }
    }
}
