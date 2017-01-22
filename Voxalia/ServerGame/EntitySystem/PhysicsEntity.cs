//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using System;
using Voxalia.Shared;
using BEPUutilities;
using Voxalia.ServerGame.JointSystem;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using BEPUphysics;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.ServerGame.NetworkSystem;
using System.Threading;
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    /// <summary>
    /// Represents a standard serverside entity that interacts with the physics world.
    /// </summary>
    public abstract class PhysicsEntity: Entity
    {
        public bool TransmitMe = true;

        public bool GenBlockShadow = false;

        /// <summary>
        /// Construct the physics entity.
        /// Sets its gravity to the world default and collisiongroup to Solid.
        /// </summary>
        /// <param name="tregion">The region the entity will be spawned in.</param>
        public PhysicsEntity(Region tregion)
            : base(tregion, true)
        {
            Gravity = new Location(TheRegion.PhysicsWorld.ForceUpdater.Gravity);
        }

        public override double GetScaleEstimate()
        {
            if (ScaleEst < 0)
            {
                if (Body == null)
                {
                    return 1;
                }
                BoundingBox bb = Body.CollisionInformation.BoundingBox;
                ScaleEst = Vector3.Max(bb.Max, -bb.Min).Length();
            }
            return ScaleEst;
        }

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + 200;
        }

        public const int PhysicsNetLength = 4 + 24 + 24 + 16 + 24 + 4 + 4 + 1 + 1;

        public byte[] GetPhysicsNetData()
        {
            byte[] Data = new byte[PhysicsNetLength];
            Utilities.FloatToBytes((float)GetMass()).CopyTo(Data, 0);
            GetPosition().ToDoubleBytes().CopyTo(Data, 4);
            GetVelocity().ToDoubleBytes().CopyTo(Data, 4 + 24);
            Utilities.QuaternionToBytes(GetOrientation()).CopyTo(Data, 4 + 24 + 24);
            GetAngularVelocity().ToDoubleBytes().CopyTo(Data, 4 + 24 + 24 + 16);
            Utilities.FloatToBytes((float)GetFriction()).CopyTo(Data, 4 + 24 + 24 + 16 + 24);
            Utilities.FloatToBytes((float)GetBounciness()).CopyTo(Data, 4 + 24 + 24 + 16 + 24 + 4);
            // TODO: Proper flags thingy here?
            Data[4 + 24 + 24 + 16 + 24 + 4 + 4] = (byte)((Visible ? 1 : 0) | (GenBlockShadow ? 2 : 0));
            byte cg = 0;
            if (CGroup == CollisionUtil.Solid)
            {
                cg = 2;
            }
            else if (CGroup == CollisionUtil.NonSolid)
            {
                cg = 4;
            }
            else if (CGroup == CollisionUtil.Item)
            {
                cg = 2 | 4;
            }
            else if (CGroup == CollisionUtil.Player)
            {
                cg = 8;
            }
            else if (CGroup == CollisionUtil.Water)
            {
                cg = 2 | 8;
            }
            else if (CGroup == CollisionUtil.WorldSolid)
            {
                cg = 2 | 4 | 8;
            }
            else if (CGroup == CollisionUtil.Character)
            {
                cg = 16;
            }
            Data[4 + 24 + 24 + 16 + 24 + 4 + 4 + 1] = cg;
            return Data;
        }

        /// <summary>
        /// The widest this entity gets at its furthest corner. Can be used to generate a bounding sphere.
        /// </summary>
        public double Widest = 1;

        /// <summary>
        /// All information on the physical version of this entity as it exists within the physics world.
        /// </summary>
        public BEPUphysics.Entities.Entity Body = null;

        /// <summary>
        /// The mass of the entity.
        /// </summary>
        internal double Mass = 0f;

        /// <summary>
        /// The friction value of the entity.
        /// </summary>
        internal double Friction = 0.5f;

        /// <summary>
        /// The bounciness (restitution coefficient) of the entity.
        /// </summary>
        internal double Bounciness = 0.25f;

        /// <summary>
        /// The gravity power of this entity.
        /// </summary>
        internal Location Gravity;

        /// <summary>
        /// Whether this entity can rotate. Only updated at SpawnBody().
        /// </summary>
        public bool CanRotate = true;

        /// <summary>
        /// The position, rotation, etc. of this entity.
        /// </summary>
        internal Matrix WorldTransform = Matrix.Identity;

        /// <summary>
        /// The linear velocity of this entity.
        /// </summary>
        internal Location LVel = Location.Zero;

        /// <summary>
        /// The angular velocity of this entity.
        /// </summary>
        internal Location AVel = Location.Zero;

        /// <summary>
        /// The shape of the entity.
        /// </summary>
        public EntityShape Shape = null;
        
        public Location InternalOffset;

        /// <summary>
        /// What collision group this entity belongs to.
        /// </summary>
        public CollisionGroup CGroup = CollisionUtil.Solid;
        
        public bool netpActive = false;

        public double netdeltat = 0;

        public Location lPos = Location.NaN;

        public double ScaleEst = -1;

        public void PreTick()
        {
            NetworkTick();
        }
        
        /// <summary>
        /// Ticks the physics entity, doing nothing at all.
        /// </summary>
        public override void Tick()
        {
            if (!TheRegion.IsVisible(GetPosition()))
            {
                if (Body.ActivityInformation.IsActive)
                {
                    wasActive = true;
                    // TODO: Is this needed?
                    if (Body.ActivityInformation.SimulationIsland != null)
                    {
                        Body.ActivityInformation.SimulationIsland.IsActive = false;
                    }
                }
            }
            else if (wasActive)
            {
                wasActive = false;
                Body.ActivityInformation.Activate();
            }
            Vector3i cpos = TheRegion.ChunkLocFor(GetPosition());
            if (CanSave && !TheRegion.LoadedChunks.ContainsKey(cpos))
            {
                TheRegion.LoadChunk(cpos);
            }
        }

        bool wasActive = false;

        public override void PotentialActivate()
        {
            if (Body != null)
            {
                Body.ActivityInformation.Activate();
            }
        }

        bool wasEverVis = false;

        void NetworkTick()
        {
            if (NetworkMe)
            {
                bool sme = needNetworking;
                needNetworking = false;
                // TODO: Timer of some form, to prevent packet flood on a speedy server?
                if (Body != null && (Body.ActivityInformation.IsActive || (netpActive && !Body.ActivityInformation.IsActive)))
                {
                    netpActive = Body.ActivityInformation.IsActive;
                    sme = true;
                }
                if (!netpActive && GetMass() > 0)
                {
                    netdeltat += TheRegion.Delta;
                    if (netdeltat > 2.0)
                    {
                        netdeltat -= 2.0;
                        sme = true;
                    }
                }
                Location pos = GetPosition();
                if (!TransmitMe)
                {
                    sme = false;
                }
                AbstractPacketOut physupd = sme ? GetUpdatePacket() : null;
                foreach (PlayerEntity player in TheRegion.Players)
                {
                    if (player == this)
                    {
                        continue;
                    }
                    bool shouldseec = player.ShouldSeePosition(pos);
                    bool shouldseel = player.Known.Contains(EID); // player.ShouldSeePositionPreviously(lPos) && wasEverVis;
                    if (shouldseec && !shouldseel)
                    {
                        player.Known.Add(EID);
                        player.Network.SendPacket(GetSpawnPacket());
                        foreach (InternalBaseJoint joint in Joints)
                        {
                            if (player.ShouldSeePosition(joint.One.GetPosition()) && player.ShouldSeePosition(joint.Two.GetPosition()))
                            {
                                player.Network.SendPacket(new AddJointPacketOut(joint));
                            }
                        }
                    }
                    if (shouldseel && !shouldseec)
                    {
                        player.Network.SendPacket(new DespawnEntityPacketOut(EID));
                        player.Known.Remove(EID);
                    }
                    if (sme && shouldseec)
                    {
                        player.Network.SendPacket(physupd);
                    }
                    // TODO
                    /*
                    if (!shouldseec)
                    {
                        bool shouldseelongc = player.ShouldLoadPosition(pos);
                        bool shouldseelongl = player.ShouldLoadPositionPreviously(lPos);
                        if (shouldseelongc && !shouldseelongl)
                        {
                            AbstractPacketOut lod = GetLODSpawnPacket();
                            if (lod != null)
                            {
                                player.Network.SendPacket(lod);
                            }
                        }
                        if (shouldseelongl && !shouldseelongc)
                        {
                            player.Network.SendPacket(new DespawnEntityPacketOut(EID));
                        }
                    }
                    */
                }
                wasEverVis = true;
            }
        }

        public virtual void EndTick()
        {
            lPos = GetPosition();
        }

        public virtual AbstractPacketOut GetLODSpawnPacket()
        {
            return null;
        }

        public virtual AbstractPacketOut GetUpdatePacket()
        {
            return new PhysicsEntityUpdatePacketOut(this);
        }
        
        bool needNetworking = false;
        
        public void ForceNetwork()
        {
            needNetworking = true;
            NetworkTick();
            EndTick();
        }

        /// <summary>
        /// Builds and spawns the body into the world.
        /// </summary>
        public virtual void SpawnBody()
        {
            if (Body != null)
            {
                DestroyBody();
            }
            Body = new BEPUphysics.Entities.Entity(Shape, Mass);
            Body.CollisionInformation.CollisionRules.Group = CGroup;
            InternalOffset = new Location(Body.Position);
            Body.AngularVelocity = new Vector3((double)AVel.X, (double)AVel.Y, (double)AVel.Z);
            Body.LinearVelocity = new Vector3((double)LVel.X, (double)LVel.Y, (double)LVel.Z);
            Body.WorldTransform = WorldTransform; // TODO: Position, Orientation
            Body.Tag = this;
            Body.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Passive;
            if (!CanRotate)
            {
                Body.AngularDamping = 1;
            }
            // TODO: Other settings
            // TODO: Gravity
            SetFriction(Friction);
            SetBounciness(Bounciness);
            TheRegion.PhysicsWorld.Add(Body);
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] is BaseJoint)
                {
                    BaseJoint joint = (BaseJoint)Joints[i];
                    joint.CurrentJoint = joint.GetBaseJoint();
                    TheRegion.PhysicsWorld.Add(joint.CurrentJoint);
                }
            }
        }

        /// <summary>
        /// Destroys the body, removing it from the physics world.
        /// </summary>
        public virtual void DestroyBody()
        {
            if (Seats != null)
            {
                foreach (Seat seat in Seats)
                {
                    if (seat.Sitter != null)
                    {
                        seat.Kick();
                    }
                }
            }
            LVel = new Location(Body.LinearVelocity.X, Body.LinearVelocity.Y, Body.LinearVelocity.Z);
            AVel = new Location(Body.AngularVelocity.X, Body.AngularVelocity.Y, Body.AngularVelocity.Z);
            Friction = GetFriction();
            // TODO: Gravity = new Location(Body.Gravity.X, Body.Gravity.Y, Body.Gravity.Z);
            WorldTransform = Body.WorldTransform;
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] is BaseJoint)
                {
                    BaseJoint joint = (BaseJoint)Joints[i];
                    try
                    {
                        TheRegion.PhysicsWorld.Remove(joint.CurrentJoint);
                    }
                    catch (Exception ex)
                    {
                        // We don't actually care if this errors.
                        Utilities.CheckException(ex);
                    }
                }
            }
            TheRegion.PhysicsWorld.Remove(Body);
            Body = null;
        }

        /// <summary>
        /// Returns the friction level of this entity.
        /// </summary>
        public virtual double GetFriction()
        {
            if (Body == null)
            {
                return Friction;
            }
            return Body.Material.KineticFriction;
        }

        /// <summary>
        /// Sets the friction level of this entity.
        /// </summary>
        /// <param name="fric">The friction level.</param>
        public virtual void SetFriction(double fric)
        {
            Friction = fric;
            if (Body != null)
            {
                // TODO: Separate
                Body.Material.StaticFriction = fric;
                Body.Material.KineticFriction = fric;
            }
        }

        /// <summary>
        /// Returns the bounciness (restitution coefficient) of this entity.
        /// </summary>
        public virtual double GetBounciness()
        {
            if (Body == null)
            {
                return Bounciness;
            }
            return Body.Material.Bounciness;
        }

        /// <summary>
        /// Sets the bounciness (restitution coefficient)  of this entity.
        /// </summary>
        /// <param name="bounce">The bounciness (restitution coefficient) .</param>
        public virtual void SetBounciness
            (double bounce)
        {
            Bounciness = bounce;
            if (Body != null)
            {
                Body.Material.Bounciness = bounce;
            }
        }

        /// <summary>
        /// Returns the mass of the entity.
        /// </summary>
        public virtual double GetMass()
        {
            return Body == null ? Mass : Body.Mass;
        }

        /// <summary>
        /// Sets the mass of this entity.
        /// </summary>
        /// <param name="mass">The new mass value.</param>
        public virtual void SetMass(double mass)
        {
            Mass = mass;
            if (Body != null)
            {
                Body.Mass = mass;
            }
        }

        /// <summary>
        /// Returns the position of this entity within the world.
        /// </summary>
        public override Location GetPosition()
        {
            if (Body == null)
            {
                return new Location(WorldTransform.Translation.X, WorldTransform.Translation.Y, WorldTransform.Translation.Z);
            }
            Vector3 pos = Body.Position;
            return new Location(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Sets the position of this entity within the world.
        /// </summary>
        /// <param name="pos">The position to move the entity to.</param>
        public override void SetPosition(Location pos)
        {
            needNetworking = true;
            if (Body != null)
            {
                Body.Position = pos.ToBVector();
            }
            else
            {
                WorldTransform.Translation = pos.ToBVector();
            }
        }

        /// <summary>
        /// Returns the velocity of this entity.
        /// </summary>
        public virtual Location GetVelocity()
        {
            if (Body != null)
            {
                Vector3 vel = Body.LinearVelocity;
                return new Location(vel.X, vel.Y, vel.Z);
            }
            else
            {
                return LVel;
            }
        }

        /// <summary>
        /// Sets the velocity of this entity.
        /// </summary>
        /// <param name="vel">The new velocity.</param>
        public virtual void SetVelocity(Location vel)
        {
            LVel = vel;
            if (Body != null)
            {
                Body.LinearVelocity = vel.ToBVector();
            }
        }

        /// <summary>
        /// Returns the angular velocity of this entity.
        /// </summary>
        public virtual Location GetAngularVelocity()
        {
            if (Body != null)
            {
                Vector3 vel = Body.AngularVelocity;
                return new Location(vel.X, vel.Y, vel.Z);
            }
            else
            {
                return AVel;
            }
        }

        /// <summary>
        /// Sets the angular velocity of this entity.
        /// </summary>
        /// <param name="vel">The new velocity.</param>
        public virtual void SetAngularVelocity(Location vel)
        {
            AVel = vel;
            if (Body != null)
            {
                Body.AngularVelocity = vel.ToBVector();
            }
        }

        /// <summary>
        /// Returns the orientation of an entity.
        /// </summary>
        public override Quaternion GetOrientation()
        {
            if (Body != null)
            {
                return Body.Orientation;
            }
            return Quaternion.CreateFromRotationMatrix(WorldTransform);
        }

        /// <summary>
        /// Sets the direction of the entity.
        /// </summary>
        /// <param name="rot">The new angles.</param>
        public override void SetOrientation(Quaternion rot)
        {
            if (Body != null)
            {
                Body.Orientation = rot;
            }
            else
            {
                WorldTransform = Matrix.CreateFromQuaternion(rot) * Matrix.CreateTranslation(WorldTransform.Translation);
            }
        }

        /// <summary>
        /// Returns the gravity value for this physics entity, if it has one.
        /// Otherwise, returns the body's default gravity.
        /// </summary>
        /// <returns>The gravity value.</returns>
        public Location GetGravity()
        {
            if (Body != null && Body.Gravity.HasValue)
            {
                return new Location(Body.Gravity.Value);
            }
            return Gravity;
        }

        /// <summary>
        /// Sets the gravity value for this physics entity.
        /// </summary>
        /// <param name="gravity">The gravity value.</param>
        public void SetGravity(Location gravity)
        {
            Gravity = gravity;
            if (Body != null)
            {
                Body.Gravity = gravity.ToBVector();
            }
        }

        /// <summary>
        /// Applies a force directly to the physics entity's body.
        /// The force is assumed to be perfectly central to the entity.
        /// Note: this is a force, not a velocity. Mass is relevant.
        /// This will activate the entity.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void ApplyForce(Location force)
        {
            if (Body != null)
            {
                Vector3 vec = force.ToBVector();
                Body.ApplyLinearImpulse(ref vec);
                Body.ActivityInformation.Activate();
            }
            else
            {
                LVel += force / Mass;
            }
        }

        /// <summary>
        /// Applies a force directly to the physics entity's body, at a specified relative origin point.
        /// The origin is relevant to the body's centerpoint.
        /// The further you get from the centerpoint, the more spin and less linear motion will be applied.
        /// Note: this is a force, not a velocity. Mass is relevant.
        /// This will activate the entity.
        /// </summary>
        /// <param name="force">The force to apply.</param>
        public void ApplyForce(Location origin, Location force)
        {
            if (Body != null)
            {
                Vector3 ori = origin.ToBVector();
                Vector3 vec = force.ToBVector();
                Body.ApplyImpulse(ref ori, ref vec);
                Body.ActivityInformation.Activate();
            }
            else
            {
                // TODO: Account for spin?
                LVel += force / Mass;
            }
        }
        
        public void AddPhysicsData(BsonDocument doc)
        {
            doc["ph_pos"] = GetPosition().ToDoubleBytes();
            doc["ph_vel"] = GetVelocity().ToDoubleBytes();
            doc["ph_avel"] = GetAngularVelocity().ToDoubleBytes();
            // TODO: Quat-to-bytes system!
            Quaternion quat = GetOrientation();
            doc["ph_ang_x"] = (double)quat.X;
            doc["ph_ang_y"] = (double)quat.Y;
            doc["ph_ang_z"] = (double)quat.Z;
            doc["ph_ang_w"] = (double)quat.W;
            doc["ph_grav"] = GetGravity().ToDoubleBytes();
            doc["ph_bounce"] = (double)GetBounciness();
            doc["ph_frict"] = (double)GetFriction();
            doc["ph_mass"] = (double)GetMass();
            // TODO: Separate method for CG.
            int cg = 0;
            if (CGroup == CollisionUtil.NonSolid)
            {
                cg = 0;
            }
            else if (CGroup == CollisionUtil.Solid)
            {
                cg = 1;
            }
            else if (CGroup == CollisionUtil.Player)
            {
                cg = 2;
            }
            else if (CGroup == CollisionUtil.Item)
            {
                cg = 3;
            }
            else if (CGroup == CollisionUtil.Water)
            {
                cg = 4;
            }
            doc["ph_cg"] = cg;
            // TODO: Actual flag enum
            int flags = (Visible ? 1 : 0) | (GenBlockShadow ? 2 : 0) | (TransmitMe ? 128 : 0);
            doc["ph_flag"] = flags;
        }

        public void ApplyPhysicsData(BsonDocument doc)
        {
            if (doc.ContainsKey("ph_pos"))
            {
                SetPosition(Location.FromDoubleBytes(doc["ph_pos"].AsBinary, 0));
            }
            if (doc.ContainsKey("ph_vel"))
            {
                SetVelocity(Location.FromDoubleBytes(doc["ph_vel"].AsBinary, 0));
            }
            if (doc.ContainsKey("ph_avel"))
            {
                SetAngularVelocity(Location.FromDoubleBytes(doc["ph_avel"].AsBinary, 0));
            }
            if (doc.ContainsKey("ph_ang_x") && doc.ContainsKey("ph_ang_y") && doc.ContainsKey("ph_ang_z") && doc.ContainsKey("ph_ang_w"))
            {
                double ax = (double)doc["ph_ang_x"].AsDouble;
                double ay = (double)doc["ph_ang_y"].AsDouble;
                double az = (double)doc["ph_ang_z"].AsDouble;
                double aw = (double)doc["ph_ang_w"].AsDouble;
                SetOrientation(new Quaternion(ax, ay, az, aw));
            }
            if (doc.ContainsKey("ph_grav"))
            {
                SetGravity(Location.FromDoubleBytes(doc["ph_grav"].AsBinary, 0));
            }
            if (doc.ContainsKey("ph_bounce"))
            {
                SetBounciness((double)doc["ph_bounce"].AsDouble);
            }
            if (doc.ContainsKey("ph_frict"))
            {
                SetFriction((double)doc["ph_frict"].AsDouble);
            }
            if (doc.ContainsKey("ph_mass"))
            {
                SetMass((double)doc["ph_mass"].AsDouble);
            }
            if (doc.ContainsKey("ph_cg"))
            {
                int cg = doc["ph_cg"].AsInt32;
                if (cg == 0)
                {
                    CGroup = CollisionUtil.NonSolid;
                }
                else if (cg == 1)
                {
                    CGroup = CollisionUtil.Solid;
                }
                else if (cg == 2)
                {
                    CGroup = CollisionUtil.Player;
                }
                else if (cg == 3)
                {
                    CGroup = CollisionUtil.Item;
                }
                else // cg == 4
                {
                    CGroup = CollisionUtil.Water;
                }
            }
            if (doc.ContainsKey("ph_flag"))
            {
                int flags = doc["ph_flag"].AsInt32;
                Visible = (flags & 1) == 1;
                GenBlockShadow = (flags & 2) == 2;
                TransmitMe = (flags & 128) == 128;

            }
        }
    }
}
