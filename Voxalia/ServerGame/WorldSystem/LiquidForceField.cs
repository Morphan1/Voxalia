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

namespace Voxalia.ServerGame.WorldSystem
{
    class LiquidForceField : ForceField // TODO: Rework to have its own update system, instead of relying on ForceField?s
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
            if (e.Mass <= 0)
            {
                impulse = Vector3.Zero;
                return;
            }
            Location min = new Location(e.CollisionInformation.BoundingBox.Min);
            Location max = new Location(e.CollisionInformation.BoundingBox.Max);
            if (TheRegion.InWater(min, max))
            {
                double vol = e.CollisionInformation.Shape.Volume; // TODO: Accuracy! Perhaps, based on how much is submerged in the current blocK?
                double dens = (e.Mass / vol);
                double WaterDens = 5; // TODO: Read from material. // TODO: Sanity of values.
                float modifier = (float)(WaterDens / dens);
                // TODO: Tracing accuracy! For-each-contained-liquid-block, apply a force at that location.
                impulse = -(TheRegion.PhysicsWorld.ForceUpdater.Gravity + TheRegion.GravityNormal.ToBVector() * 0.4f) * e.Mass * dt * modifier;
                e.ModifyLinearDamping(0.5f); // TODO: Modifier?
                e.ModifyAngularDamping(0.5f);
            }
            else
            {
                impulse = Vector3.Zero;
            }
        }
    }
}
