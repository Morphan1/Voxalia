//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using OpenTK;
using System;

namespace Voxalia.ClientGame.EntitySystem
{
    public abstract class PrimitiveEntity: Entity
    {
        public PrimitiveEntity(Region tregion, bool cast_shadows)
            : base(tregion, true, cast_shadows)
        {
        }

        public Location Gravity = Location.Zero;

        public List<long> NoCollide = new List<long>(); // TODO: Populate me via packets

        public bool FilterHandle(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            if (entry is BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)
            {
                long eid = ((PhysicsEntity)((BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)entry).Entity.Tag).EID;
                if (NoCollide.Contains(eid))
                {
                    return false;
                }
            }
            if (entry.CollisionRules.Group == CollisionUtil.NonSolid)
            {
                return false;
            }
            return true;
        }

        public override void Tick()
        {
            SetVelocity(Velocity * 0.99f + Gravity * TheClient.Delta);
            if (Velocity.LengthSquared() > 0)
            {
                CollisionResult cr = TheClient.TheRegion.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + Velocity * TheClient.Delta, FilterHandle);
                SetPosition(cr.Position);
            }
        }

        public abstract void Spawn();

        public abstract void Destroy();

        public Location Position;

        public Location Velocity;

        public BEPUutilities.Quaternion Angles;

        public Matrix4d GetOrientationMatrix()
        {
            return Matrix4d.CreateFromQuaternion(new Quaterniond(Angles.X, Angles.Y, Angles.Z, Angles.W));
        }

        public override BEPUutilities.Quaternion GetOrientation()
        {
            return Angles;
        }

        public override void SetOrientation(BEPUutilities.Quaternion quat)
        {
            Angles = quat;
        }

        public Location Scale;

        public override Location GetPosition()
        {
            return Position;
        }

        public override void SetPosition(Location pos)
        {
            Position = pos;
        }

        public virtual void SetVelocity(Location vel)
        {
            Velocity = vel;
        }
    }
}
