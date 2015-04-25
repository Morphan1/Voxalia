using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public abstract class PrimitiveEntity: Entity
    {
        public List<long> NoCollide = new List<long>();

        public PrimitiveEntity(Server tserver)
            : base(tserver, true)
        {
        }

        public override void Tick()
        {
            SetPosition(Position + Velocity * TheServer.Delta);
            // TODO: Collision? Gravity?
        }

        public virtual void Spawn()
        {
            NoCollide.Add(EID);
        }

        public virtual void Destroy()
        {
            NoCollide.Remove(EID);
        }

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

        public Location GetVelocity()
        {
            return Velocity;
        }

        public virtual void SetVelocity(Location vel)
        {
            Velocity = vel;
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            return base.GetVariables();
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "angle":
                    return true; // TODO
                case "angular_velocity":
                    return true; // TODO
                case "mass":
                    return true; // Ignore
                case "friction":
                    return true; // Ignore
                case "velocity":
                    SetVelocity(Location.FromString(data));
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }
    }
}
