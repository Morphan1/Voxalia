using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public Region TheRegion;

        public Entity(Region tregion, bool tickme)
        {
            TheRegion = tregion;
            TheServer = tregion.TheServer;
            Ticks = tickme;
        }

        public bool NetworkMe = true; // TODO: Readonly? Toggler method?

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public long EID;

        /// <summary>
        /// The ID used to identify this entity to joints.
        /// </summary>
        public string JointTargetID = "none";

        /// <summary>
        /// Whether this entity should tick.
        /// </summary>
        public readonly bool Ticks;

        /// <summary>
        /// Whether the entity is spawned into the world.
        /// </summary>
        public bool IsSpawned = false;

        /// <summary>
        /// The seat this entity is currently sitting in.
        /// </summary>
        public Seat CurrentSeat = null;
        
        /// <summary>
        /// The seats available to sit in on this entity.
        /// </summary>
        public List<Seat> Seats = null;

        /// <summary>
        /// The server that manages this entity.
        /// </summary>
        public Server TheServer = null;

        public List<InternalBaseJoint> Joints = new List<InternalBaseJoint>();

        /// <summary>
        /// Tick the entity. Default implementation throws exception.
        /// </summary>
        public virtual void Tick()
        {
            throw new NotImplementedException();
        }

        public abstract Location GetPosition();

        public abstract void SetPosition(Location pos);

        public abstract BEPUutilities.Quaternion GetOrientation();

        public abstract void SetOrientation(BEPUutilities.Quaternion quat);

        public bool Visible = true;

        public abstract EntityType GetEntityType();

        public abstract byte[] GetSaveBytes();
    }

    public abstract class EntityConstructor
    {
        public abstract Entity Create(Region tregion, byte[] input);
    }
}
