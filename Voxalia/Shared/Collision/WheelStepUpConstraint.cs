using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUutilities;

namespace Voxalia.Shared.Collision
{
    public class WheelStepUpConstraint : SingleEntityConstraint
    {
        public WheelStepUpConstraint(Entity e, CollisionUtil collis, float height)
        {
            Entity = e;
            HopHeight = height;
            Collision = collis;
        }

        CollisionUtil Collision;
        float HopHeight;
        bool NeedsHop;
        Vector3 Hop;

        public override void ExclusiveUpdate()
        {
            if (NeedsHop)
            {
                Entity.Position += Hop;
            }
        }

        public override float SolveIteration()
        {
            return 0; // TODO: ???
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity == Entity)
            {
                return false;
            }
            return Collision.ShouldCollide(entry);
        }

        public override void Update(float dt)
        {
            NeedsHop = false;
            Entity e = Entity;
            Vector3 vel = e.LinearVelocity * dt;
            RigidTransform start = new RigidTransform(e.Position + new Vector3(0, 0, 0.05f), e.Orientation);
            RayCastResult rcr;
            if (e.Space.ConvexCast((ConvexShape)e.CollisionInformation.Shape, ref start, ref vel, IgnoreThis, out rcr))
            {
                vel += new Vector3(0, 0, HopHeight);
                if (!e.Space.ConvexCast((ConvexShape)e.CollisionInformation.Shape, ref start, ref vel, IgnoreThis, out rcr))
                {
                    start.Position += vel;
                    vel = new Vector3(0, 0, -(HopHeight + 0.05f)); // TODO: Track gravity normals and all that stuff
                    if (e.Space.ConvexCast((ConvexShape)e.CollisionInformation.Shape, ref start, ref vel, IgnoreThis, out rcr))
                    {
                        NeedsHop = true;
                        Hop = -vel * (1f - rcr.HitData.T / (HopHeight + 0.05f));
                    }
                }
            }
        }
    }
}
