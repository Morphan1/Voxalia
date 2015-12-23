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
using BEPUphysics.UpdateableSystems;
using Voxalia.Shared;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;
using BEPUutilities.DataStructures;

namespace Voxalia.ServerGame.WorldSystem
{
    public class LiquidVolume : Updateable, IDuringForcesUpdateable
    {
        public Region TheRegion;

        public LiquidVolume(Region tregion)
        {
            TheRegion = tregion;
        }

        public void Update(float dt)
        {
            ReadOnlyList<Entity> ents = TheRegion.PhysicsWorld.Entities; // TODO: Direct/raw read?
            TheRegion.PhysicsWorld.ParallelLooper.ForLoop(0, ents.Count, (i) =>
            {
                ApplyLiquidForcesTo(ents[i], dt);
            });
        }

        void ApplyLiquidForcesTo(Entity e, float dt)
        {
            if (e.Mass <= 0)
            {
                return;
            }
            Location min = new Location(e.CollisionInformation.BoundingBox.Min);
            Location max = new Location(e.CollisionInformation.BoundingBox.Max);
            if (TheRegion.InWater(min, max))
            {
                double vol = e.CollisionInformation.Shape.Volume;
                double dens = (e.Mass / vol);
                double WaterDens = 5; // TODO: Read from material. // TODO: Sanity of values.
                float modifier = (float)(WaterDens / dens);
                // TODO: Tracing accuracy! For-each-contained-liquid-block, apply a force at that location.
                Vector3 impulse = -(TheRegion.PhysicsWorld.ForceUpdater.Gravity + TheRegion.GravityNormal.ToBVector() * 0.4f) * e.Mass * dt * modifier;
                e.ApplyLinearImpulse(ref impulse);
                e.ModifyLinearDamping(0.5f); // TODO: Modifier?
                e.ModifyAngularDamping(0.5f);
            }
        }
    }
}
