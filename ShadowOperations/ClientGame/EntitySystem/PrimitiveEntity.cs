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
        public PrimitiveEntity(Client tclient)
            : base(tclient, true)
        {
        }

        public override void Tick()
        {
            Position += Velocity;
            // TODO: Collision? Gravity?
        }

        public abstract void Spawn();

        public abstract void Destroy();

        public Location Position;

        public Location Velocity;

        public override Location GetPosition()
        {
            return Position;
        }

        public override void SetPosition(Location pos)
        {
            Position = pos;
        }
    }
}
