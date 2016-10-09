//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.NetworkSystem;

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

        public HashSet<long> NoCollide = new HashSet<long>();

        public abstract string GetModel();

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + 50 + NoCollide.Count * 16;
        }

        public PrimitiveEntity(Region tregion)
            : base(tregion, true)
        {
        }

        public const int PrimitiveNetDataLength = 24 + 24 + 16 + 24 + 24 + 4;

        public byte[] GetPrimitiveNetData()
        {
            byte[] Data = new byte[PrimitiveNetDataLength];
            GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            GetVelocity().ToDoubleBytes().CopyTo(Data, 24);
            Utilities.QuaternionToBytes(Angles).CopyTo(Data, 24 + 24);
            Scale.ToDoubleBytes().CopyTo(Data, 24 + 24 + 16);
            Gravity.ToDoubleBytes().CopyTo(Data, 24 + 24 + 16 + 24);
            Utilities.IntToBytes(TheServer.Networking.Strings.IndexForString(GetModel())).CopyTo(Data, 24 + 24 + 16 + 24 + 24);
            return Data;
        }

        public override void PotentialActivate()
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
            return TheRegion.Collision.ShouldCollide(entry);
        }
        
        public double netdeltat = 0;

        public override void Tick()
        {
            if (TheRegion.IsVisible(GetPosition()))
            {
                bool sme = false;
                SetVelocity(GetVelocity() * 0.99f + Gravity * TheRegion.Delta);
                if (GetVelocity().LengthSquared() > 0)
                {
                    CollisionResult cr = TheRegion.Collision.CuboidLineTrace(Scale, GetPosition(), GetPosition() + GetVelocity() * TheRegion.Delta, FilterHandle);
                    Location vel = GetVelocity();
                    if (cr.Hit && Collide != null)
                    {
                        Collide(this, new CollisionEventArgs(cr));
                    }
                    if (!IsSpawned || Removed)
                    {
                        return;
                    }
                    if (vel == GetVelocity())
                    {
                        SetVelocity((cr.Position - GetPosition()) / TheRegion.Delta);
                    }
                    SetPosition(cr.Position);
                    // TODO: Timer
                    if (network)
                    {
                        sme = true;
                    }
                    netdeltat = 2;
                }
                else
                {
                    netdeltat += TheRegion.Delta;
                    if (netdeltat > 2.0)
                    {
                        netdeltat -= 2.0;
                        sme = true;
                    }
                }
                Location pos = GetPosition();
                PrimitiveEntityUpdatePacketOut primupd = sme ? new PrimitiveEntityUpdatePacketOut(this) : null;
                foreach (PlayerEntity player in TheRegion.Players)
                {
                    bool shouldseec = player.ShouldSeePosition(pos);
                    bool shouldseel = player.ShouldSeePositionPreviously(lPos);
                    if (shouldseec && !shouldseel)
                    {
                        player.Network.SendPacket(GetSpawnPacket());
                    }
                    if (shouldseel && !shouldseec)
                    {
                        player.Network.SendPacket(new DespawnEntityPacketOut(EID));
                    }
                    if (sme && shouldseec)
                    {
                        player.Network.SendPacket(primupd);
                    }
                }
            }
            lPos = GetPosition();
            Vector3i cpos = TheRegion.ChunkLocFor(lPos);
            if (CanSave && !TheRegion.LoadedChunks.ContainsKey(cpos))
            {
                TheRegion.LoadChunk(cpos);
            }
        }
        
        public Location lPos = Location.NaN;

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

        /// <summary>
        /// Gets the binary save data for a generic physics entity, used as part of the save procedure for a physics entity.
        /// Returns 52 bytes currently.
        /// </summary>
        /// <returns>The binary data.</returns>
        public byte[] GetPrimitiveBytes()
        {
            byte[] bytes = new byte[24 + 24 + 4 + 4 + 4 + 4 + 24];
            GetPosition().ToDoubleBytes().CopyTo(bytes, 0);
            GetVelocity().ToDoubleBytes().CopyTo(bytes, 24);
            Utilities.FloatToBytes((float)Angles.X).CopyTo(bytes, 24 + 24);
            Utilities.FloatToBytes((float)Angles.Y).CopyTo(bytes, 24 + 24 + 4);
            Utilities.FloatToBytes((float)Angles.Z).CopyTo(bytes, 24 + 24 + 4 + 4);
            Utilities.FloatToBytes((float)Angles.W).CopyTo(bytes, 24 + 24 + 4 + 4 + 4);
            Gravity.ToDoubleBytes().CopyTo(bytes, 24 + 24 + 4 + 4 + 4 + 4);
            return bytes;
        }
    }
}
