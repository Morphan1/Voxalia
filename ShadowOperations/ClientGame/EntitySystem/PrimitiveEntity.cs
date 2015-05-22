using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public abstract class PrimitiveEntity: Entity
    {
        public PrimitiveEntity(Client tclient, bool cast_shadows)
            : base(tclient, true, cast_shadows)
        {
        }

        public Location Gravity;

        public List<long> NoCollide = new List<long>(); // TODO: Populate me via packets

        public bool FilterHandle(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            long eid = ((PhysicsEntity)((BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)entry).Entity.Tag).EID;
            return !NoCollide.Contains(eid);
        }

        public override void Tick()
        {
            SetVelocity(Velocity + Gravity * TheClient.Delta);
            if (Velocity.LengthSquared() > 0)
            {
                CollisionResult cr = TheClient.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + Velocity * TheClient.Delta, FilterHandle);
                SetPosition(cr.Position);
            }
        }

        public abstract void Spawn();

        public abstract void Destroy();

        public Location Position;

        public Location Velocity;

        public BEPUutilities.Quaternion Angles;

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
