using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.CollisionTests;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class SlimeEntity: CharacterEntity
    {
        public SlimeEntity(Region tregion)
            : base(tregion, 20)
        {
            CBHHeight = 0.3f * 0.5f;
            CBStepHeight = 0.1f;
            CBDownStepHeight = 0.1f;
            CBRadius = 0.3f;
            SetMass(10);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.SLIME;
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.CollisionInformation.Events.ContactCreated += Events_ContactCreated;
            Body.CollisionInformation.CollisionRules.Group = CollisionUtil.Solid;
        }

        private void Events_ContactCreated(EntityCollidable sender, Collidable other, CollidablePairHandler pair, ContactData contact)
        {
            if (ApplyDamage > 0)
            {
                return;
            }
            if (!(other is EntityCollidable))
            {
                return;
            }
            PhysicsEntity pe = (PhysicsEntity)((EntityCollidable)other).Entity.Tag;
            if (!(pe is EntityDamageable))
            {
                return;
            }
            ((EntityDamageable)pe).Damage(DamageAmt);
            ApplyDamage = DamageDelay;
        }

        public float DamageAmt = 5;

        public double DamageDelay = 1;

        public double ApplyDamage = 0;

        public override void Tick()
        {
            if (CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
            }
            if (ApplyDamage > 0)
            {
                ApplyDamage -= TheRegion.Delta;
            }
            base.Tick();
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save/load!
            return null;
        }

        public override void Die()
        {
            // TODO: Death effect!
            RemoveMe();
        }

        public override Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, 0.3);
        }
    }
}
