using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    class ExplodobowItem: BowItem
    {
        public ExplodobowItem()
        {
            Name = "explodobow";
        }

        public override ArrowEntity SpawnArrow(PlayerEntity player, ItemStack item, double timeStretched)
        {
            ArrowEntity ae = base.SpawnArrow(player, item, timeStretched);
            ae.Collide += (o, o2) => { ae.TheRegion.Explode(ae.GetPosition()); };
            return ae;
        }
    }
}
