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
    public class FlyingDiscConstraint : SingleEntityConstraint
    {
        public FlyingDiscConstraint(Entity e)
        {
            Entity = e;
        }

        public override void ExclusiveUpdate()
        {
            if (!Entity.ActivityInformation.IsActive)
            {
                return;
            }
            Entity.LinearVelocity += cForce;
        }

        public override double SolveIteration()
        {
            return 0; // TODO: ???
        }

        Vector3 cForce = Vector3.Zero;

        /// <summary>
        /// Is this a flying disc, or a plane's wings?
        /// </summary>
        public bool IsAPlane = false;

        public override void Update(double dt)
        {
            if (!Entity.ActivityInformation.IsActive)
            {
                return;
            }
            // Note: Assuming Z is the axis of the flat plane of the disc.
            // TODO: Don't assume this!
            Vector3 up = Quaternion.Transform(Vector3.UnitZ, Entity.Orientation);
            double projectedZVel = Vector3.Dot(entity.LinearVelocity + entity.Gravity ?? entity.Space.ForceUpdater.Gravity, up);
            /*if (IsAPlane)
            {
                // Note: Assuming Y is the axis of the forward vector of the plane.
                // TODO: Don't assume this!
                Vector3 forw = Quaternion.Transform(Vector3.UnitY, Entity.Orientation);
                double fspeed = Vector3.Dot(entity.LinearVelocity, forw) * 0.1;
                cForce = ((fspeed - projectedZVel) * dt * 0.75) * up;
            }
            else*/
            {
                double velLen = 1f - ((1f / Math.Max(entity.LinearVelocity.LengthSquared(), 1f)));
                cForce = up * (projectedZVel * -velLen * dt * 0.75); // TODO: Arbitrary constant!
            }
        }
    }
}
