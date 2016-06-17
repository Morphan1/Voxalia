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
        public abstract float GetStrength();

        /// <summary>
        /// The default range of the item, prior to ItemStack-level adjustments.
        /// </summary>
        float RangeBase = 10;

        /// <summary>
        /// The default strength of the item, prior to ItemStack-level adjustments.
        /// </summary>
        float StrengthBase = 15;

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is CharacterEntity))
            {
                // TODO: Non-character support?
                return;
            }
            CharacterEntity character = (CharacterEntity)entity;
            float range = RangeBase * item.GetAttributeF("range_mod", 1f);
            float strength = StrengthBase * item.GetAttributeF("strength_mod", 1f) * GetStrength();
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
