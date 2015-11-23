using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
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

        public PrimitiveEntity(Region tregion)
            : base(tregion, true)
        {
        }

        public bool network = true;

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
            if (entry.CollisionRules.Group == CollisionUtil.NonSolid
                || entry.CollisionRules.Group == CollisionUtil.Trigger)
            {
                return false;
            }
            return true;
        }

        public override void Tick()
        {
            SetVelocity(GetVelocity() * 0.99f + Gravity * TheRegion.Delta);
            if (GetVelocity().LengthSquared() > 0)
            {
                CollisionResult cr = TheRegion.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + GetVelocity() * TheRegion.Delta, FilterHandle);
                Location vel = GetVelocity();
                if (cr.Hit && Collide != null)
                {
                    Collide(this, new CollisionEventArgs(cr));
                }
                if (IsSpawned)
                {
                    if (vel == GetVelocity())
                    {
                        SetVelocity((cr.Position - GetPosition()) / TheRegion.Delta);
                    }
                    SetPosition(cr.Position);
                    if (network && vel != GetVelocity())
                    {
                        TheRegion.SendToAll(new PrimitiveEntityUpdatePacketOut(this));
                    }
                }
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

        public BEPUutilities.Quaternion Angles;

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


        public override BEPUutilities.Quaternion GetOrientation()
        {
            return Angles;
        }

        public override void SetOrientation(BEPUutilities.Quaternion quat)
        {
            Angles = quat;
        }
    }
}
