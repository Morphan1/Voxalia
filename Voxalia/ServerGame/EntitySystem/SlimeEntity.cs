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
            CBStandSpeed = 3.0f;
            CBAirSpeed = 3.0f;
            CBAirForce = 100f;
            PathFindCloseEnough = 1f;
            SetMass(10);
            model = "mobs/slimes/slime";
            mod_xrot = -90;
            mod_color = System.Drawing.Color.FromArgb(Utilities.UtilRandom.Next(128) + 128, Utilities.UtilRandom.Next(128) + 128, Utilities.UtilRandom.Next(128) + 128);
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

        public double DamageDelay = 0.5f;

        public double ApplyDamage = 0;

        public override void Tick()
        {
            if (Math.Abs(XMove) > 0.1 || Math.Abs(YMove) > 0.1)
            {
                CBody.Jump();
            }
            if (ApplyDamage > 0)
            {
                ApplyDamage -= TheRegion.Delta;
            }
            TargetPlayers -= TheRegion.Delta;
            if (TargetPlayers <= 0)
            {
                double dist;
                PlayerEntity player = NearestPlayer(out dist);
                if (player != null && dist < MaxPathFindDistance * MaxPathFindDistance)
                {
                    GoTo(player);
                    CBody.Jump();
                    ApplyForce((player.GetCenter() - GetCenter()).Normalize() * GetMass());
                }
                else
                {
                    GoTo(null);
                }
                TargetPlayers = 1;
            }
            base.Tick();
        }

        public PlayerEntity NearestPlayer(out double distSquared)
        {
            PlayerEntity player = null;
            double distsq = double.MaxValue;
            Location p = GetCenter();
            foreach (PlayerEntity tester in TheRegion.Players)
            {
                double td = (tester.GetCenter() - p).LengthSquared();
                if (td < distsq)
                {
                    player = tester;
                    distsq = td;
                }
            }
            distSquared = distsq;
            return player;
        }

        public double TargetPlayers = 0;

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
