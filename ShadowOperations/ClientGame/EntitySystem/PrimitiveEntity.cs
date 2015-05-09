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

        public override void Tick()
        {
            SetPosition(Position + Velocity * TheClient.Delta);
            SetVelocity(Velocity + Gravity * TheClient.Delta);
            // TODO: Collision? Gravity?
        }

        public abstract void Spawn();

        public abstract void Destroy();

        public Location Position;

        public Location Velocity;

        public Location Angle;

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
