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
        public WheelStepUpConstraint(Entity e, CollisionUtil collis, double height)
        {
            Entity = e;
            HopHeight = height;
            Collision = collis;
        }

        CollisionUtil Collision;
        double HopHeight;
        bool NeedsHop;
        Vector3 Hop;

        public override void ExclusiveUpdate()
        {
            if (NeedsHop)
            {
                Entity.Position += Hop;
            }
        }

        public override double SolveIteration()
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

        public override void Update(double dt)
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
