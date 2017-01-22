//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using LiteDB;

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

        public virtual long GetRAMUsage()
        {
            return 8 + 8 + (Seats == null ? 8 : Seats.Count * 8) + 8;
        }

        public virtual double GetScaleEstimate()
        {
            return 1;
        }

        public bool NetworkMe = true; // TODO: Readonly? Toggler method?

        public abstract NetworkEntityType GetNetType();

        public abstract byte[] GetNetData();

        /// <summary>
        /// Whether this entity is allowed to save to file.
        /// </summary>
        public bool CanSave = true;

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public long EID = 0;
        
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

        public AbstractPacketOut GetSpawnPacket()
        {
            return new SpawnEntityPacketOut(this);
        }

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

        public abstract BsonDocument GetSaveData();

        public abstract void PotentialActivate();

        public bool Removed = false;

        public void RemoveMe()
        {
            if (Removed)
            {
                return;
            }
            Removed = true;
            TheRegion.DespawnQuick.Add(this);
        }

        public override string ToString()
        {
            return "{Entity of type " + GetEntityType() + "/" + GetNetType() + " at " + GetPosition() + " with ID " + EID + "}";
        }
    }

    public abstract class EntityConstructor
    {
        public abstract Entity Create(Region tregion, BsonDocument input);
    }
}
