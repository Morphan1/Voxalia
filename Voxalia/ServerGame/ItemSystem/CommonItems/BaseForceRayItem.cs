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
using System.Threading.Tasks;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public abstract class BaseForceRayItem : GenericItem
    {
        public abstract double GetStrength();

        /// <summary>
        /// The default range of the item, prior to ItemStack-level adjustments.
        /// </summary>
        double RangeBase = 10;

        /// <summary>
        /// The default strength of the item, prior to ItemStack-level adjustments.
        /// </summary>
        double StrengthBase = 15;

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is CharacterEntity))
            {
                // TODO: Non-character support?
                return;
            }
            CharacterEntity character = (CharacterEntity)entity;
            double range = RangeBase * item.GetAttributeF("range_mod", 1f);
            double strength = StrengthBase * item.GetAttributeF("strength_mod", 1f) * GetStrength();
            Location start = character.GetEyePosition(); // TODO: ItemPosition?
            Location forw = character.ForwardVector();
            Location mid = start + forw * range;
            // TODO: base the pull on extent of the entity rather than its center. IE, if the side of a big ent is targeted, it should be rotated by the force.
            List<Entity> ents = character.TheRegion.GetEntitiesInRadius(mid, range);
            foreach (Entity ent in ents)
            {
                if (ent is PhysicsEntity) // TODO: Support for primitive ents?
                {
                    PhysicsEntity pent = (PhysicsEntity)ent;
                    Location rel = (start - ent.GetPosition());
                    double distsq = rel.LengthSquared();
                    if (distsq < 1)
                    {
                        distsq = 1;
                    }
                    pent.ApplyForce((rel / distsq) * strength);
                }
            }
        }
    }
}
