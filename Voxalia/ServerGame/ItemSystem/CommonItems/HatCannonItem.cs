using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class HatCannonItem : BowItem
    {
        public HatCannonItem()
        {
            Name = "hatcannon";
        }

        public override ArrowEntity SpawnArrow(PlayerEntity player, ItemStack item, double timeStretched)
        {
            ArrowEntity arrow = base.SpawnArrow(player, item, timeStretched);
            arrow.HasHat = true;
            return arrow;
        }
    }
}
