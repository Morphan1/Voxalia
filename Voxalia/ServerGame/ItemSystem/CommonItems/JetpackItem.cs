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
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class JetpackItem: GenericItem
    {
        public JetpackItem()
        {
            Name = "jetpack";
        }

        public override void Tick(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            human.JPBoost = human.ItemLeft;
            human.JPHover = human.ItemRight;
        }
        
        public override void SwitchTo(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            bool has_fuel = human.ConsumeFuel(0);
            human.TheRegion.SendToVisible(human.GetPosition(), new FlagEntityPacketOut(human, EntityFlag.HAS_FUEL, has_fuel ? 1f : 0f));
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
            if (!(entity is HumanoidEntity))
            {
                // TODO: Non-human support?
                return;
            }
            HumanoidEntity human = (HumanoidEntity)entity;
            human.JPBoost = false;
            human.JPHover = false;
        }
    }
}
