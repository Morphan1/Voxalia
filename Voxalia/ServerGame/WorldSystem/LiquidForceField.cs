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
            if (e.Mass <= 0 || e.LinearVelocity.Z > 2f)
            {
                impulse = Vector3.Zero;
                return;
            }
            EntityShape shape = e.CollisionInformation.Shape;
            ConvexShape cshape;
            if (shape is ConvexShape)
            {
                cshape = (ConvexShape)shape;
            }
            else
            {
                BoundingBox bb = e.CollisionInformation.BoundingBox;
                cshape = new BoxShape(bb.Max.X - bb.Min.X, bb.Max.Y - bb.Min.Y, bb.Max.Z - bb.Min.Z);
            }
            RayCastResult rcr;
            // TODO: Efficiency!
            if (TheRegion.SpecialCaseConvexTrace(cshape, new Location(e.Position), new Location(0, 0, 1), 0.001f, MaterialSolidity.LIQUID, IgnoreEverythingButWater, out rcr))
            {
                impulse = -(TheRegion.PhysicsWorld.ForceUpdater.Gravity + TheRegion.GravityNormal.ToBVector() * 0.4f) * e.Mass * dt;
            }
            else
            {
                impulse = Vector3.Zero;
            }
        }
    }
}
