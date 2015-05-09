using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using BEPUphysics;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class CollisionEventArgs : EventArgs
    {
        public CollisionEventArgs(CollisionResult cr)
        {
            Info = cr;
        }

        public CollisionResult Info;
    }

    public abstract class PrimitiveEntity: Entity
    {
        public Location Gravity = Location.Zero;

        public List<long> NoCollide = new List<long>();

        public PrimitiveEntity(Server tserver)
            : base(tserver, true)
        {
        }

        public bool network = true;

        public bool FilterHandle(BEPUphysics.BroadPhaseEntries.BroadPhaseEntry entry)
        {
            long eid = ((PhysicsEntity)((BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable)entry).Entity.Tag).EID;
            return !NoCollide.Contains(eid);
        }

        public override void Tick()
        {
            SetVelocity(GetVelocity() + Gravity * TheServer.Delta);
            CollisionResult cr = TheServer.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + GetVelocity() * TheServer.Delta, FilterHandle);
            Location vel = GetVelocity();
            if (cr.Hit && Collide != null)
            {
                Collide(this, new CollisionEventArgs(cr));
            }
            if (IsSpawned)
            {
                if (vel == GetVelocity())
                {
                    SetVelocity((cr.Position - GetPosition()) / TheServer.Delta);
                }
                if (network && vel != GetVelocity())
                {
                    TheServer.SendToAll(new PrimitiveEntityUpdatePacketOut(this));
                }
                SetPosition(cr.Position);
            }
        }

        public EventHandler<CollisionEventArgs> Collide;

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

        public Location Scale;

        public Location Angles;

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
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string,string>("velocity", GetVelocity().ToString()));
            return vars;
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "angle":
                    Angles = Location.FromString(data);
                    return true; // TODO
                case "angular_velocity":
                    return true; // TODO
                case "mass":
                    return true; // Ignore
                case "friction":
                    return true; // Ignore
                case "solid":
                    return true; // Ignore
                case "bounciness":
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
