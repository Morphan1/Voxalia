using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.UpdateableSystems.ForceFields;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    class LiquidForceField : ForceField
    {
        Region TheRegion;
        
        public LiquidForceField(Region tregion): 
            base (new InfiniteForceFieldShape())
        {
            TheRegion = tregion;
        }

        bool IgnoreEverythingButWater(BroadPhaseEntry entry)
        {
            return entry.CollisionRules.Group == CollisionUtil.Water;
        }

        protected override void CalculateImpulse(Entity e, float dt, out Vector3 impulse)
        {
            if (e.Mass <= 0 || e.LinearVelocity.Z > 2f) // TODO: Gravity normal!
            {
                impulse = Vector3.Zero;
                return;
            }
            // TODO: Accuracy!
            if (TheRegion.InWater(new Location(e.CollisionInformation.BoundingBox.Min), new Location(e.CollisionInformation.BoundingBox.Max)))
            {
                impulse = -(TheRegion.PhysicsWorld.ForceUpdater.Gravity + TheRegion.GravityNormal.ToBVector() * 0.4f) * e.Mass * dt;
                // TODO: Calculate submerged amount!
                e.ModifyLinearDamping(0.5f);
                e.ModifyAngularDamping(0.5f);
            }
            else
            {
                impulse = Vector3.Zero;
            }
        }
    }
}
