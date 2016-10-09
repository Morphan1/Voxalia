//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
            ae.Collide += (o, o2) => { ae.TheRegion.Explode(ae.GetPosition(), 5); };
            return ae;
        }
    }
}
