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

namespace Voxalia.ServerGame.EntitySystem
{
    /// <summary>
    /// Represents a standard serverside entity that interacts with the physics world.
    /// </summary>
    public abstract class PhysicsEntity: Entity
    {
        public bool TransmitMe;

        /// <summary>
        /// Construct the physics entity.
        /// Sets its gravity to the world default and collisiongroup to Solid.
        /// </summary>
        /// <param name="tregion">The region the entity will be spawned in.</param>
        public PhysicsEntity(Region tregion)
            : base(tregion, true)
        {
            Gravity = new Location(TheRegion.PhysicsWorld.ForceUpdater.Gravity);
            CGroup = CollisionUtil.Solid;
        }

        /// <summary>
        /// The widest this entity gets at its furthest corner. Can be used to generate a bounding sphere.
        /// </summary>
        public float Widest = 1;

        /// <summary>
        /// All information on the physical version of this entity as it exists within the physics world.
        /// </summary>
        public BEPUphysics.Entities.Entity Body = null;

        /// <summary>
        /// The mass of the entity.
        /// </summary>
        internal float Mass = 0f;

        /// <summary>
        /// The friction value of the entity.
        /// </summary>
        internal float Friction = 0.5f;

        /// <summary>
        /// The bounciness (restitution coefficient) of the entity.
        /// </summary>
        internal float Bounciness = 0f;

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

        /// <summary>
        /// The best encompassing convex shape for the entity, for tracing.
        /// </summary>
        public ConvexShape ConvexEntityShape = null;
        
        public Location InternalOffset;

        /// <summary>
        /// What collision group this entity belongs to.
        /// </summary>
        public CollisionGroup CGroup;
        
        public bool netpActive = false;

        public double netdeltat = 0;

        public Location lPos = Location.NaN;
        
        /// <summary>
        /// Ticks the physics entity, currently only causing water float and transmitting to the network.
        /// </summary>
        public override void Tick()
        {
            if (NetworkMe)
            {
                bool sme = false;
                // TODO: Timer of some form, to prevent packet flood on a speedy server?
                if (Body.ActivityInformation.IsActive || (netpActive && !Body.ActivityInformation.IsActive))
                {
                    netpActive = Body.ActivityInformation.IsActive;
                    sme = true;
                }
                if (!netpActive && GetMass() > 0)
                {
                    netdeltat += TheRegion.Delta;
                    if (netdeltat > 2.0)
                    {
                        sme = true;
                    }
                }
                Location pos = GetPosition();
                if (!TransmitMe)
                {
                    sme = false;
                }
                PhysicsEntityUpdatePacketOut physupd = sme ? new PhysicsEntityUpdatePacketOut(this): null;
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
                        player.Network.SendPacket(physupd);
                    }
                }
            }
        }

        public void EndTick()
        {
            lPos = GetPosition();
        }

        public override AbstractPacketOut GetSpawnPacket()
        {
            return new SpawnPhysicsEntityPacketOut(this);
        }

        public void ForceNetwork()
        {
            TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
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
            if (Shape is ConvexShape)
            {
                ConvexEntityShape = (ConvexShape)Shape;
            }
            else
            {
                if (Shape is MobileMeshShape)
                {
                    MobileMeshShape mms = (MobileMeshShape)Shape;
                    RigidTransform rt = new RigidTransform(Vector3.Zero, Quaternion.Identity);
                    BoundingBox bb;
                    mms.GetBoundingBox(ref rt, out bb);
                    Vector3 size = bb.Max - bb.Min;
                    ConvexEntityShape = new BoxShape(size.X, size.Y, size.Z);
                }
            }
            Body = new BEPUphysics.Entities.Entity(Shape, Mass);
            Body.CollisionInformation.CollisionRules.Group = CGroup;
            InternalOffset = new Location(Body.Position);
            Body.AngularVelocity = new Vector3((float)AVel.X, (float)AVel.Y, (float)AVel.Z);
            Body.LinearVelocity = new Vector3((float)LVel.X, (float)LVel.Y, (float)LVel.Z);
            Body.WorldTransform = WorldTransform; // TODO: Position, Orientation
            Body.Tag = this;
            Body.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            if (!CanRotate)
            {
                Body.AngularDamping = 1;
            }
            // TODO: Other settings
            // TODO: Gravity
            SetFriction(Friction);
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
                    TheRegion.PhysicsWorld.Remove(joint.CurrentJoint);
                }
            }
            TheRegion.PhysicsWorld.Remove(Body);
            Body = null;
        }

        /// <summary>
        /// Returns the friction level of this entity.
        /// </summary>
        public virtual float GetFriction()
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
        public virtual void SetFriction(float fric)
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
        public virtual float GetBounciness()
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
            (float bounce)
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
        public virtual float GetMass()
        {
            return Body == null ? Mass : Body.Mass;
        }

        /// <summary>
        /// Sets the mass of this entity.
        /// </summary>
        /// <param name="mass">The new mass value.</param>
        public virtual void SetMass(float mass)
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
            Vector3 pos = Body.WorldTransform.Translation;
            return new Location(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Sets the position of this entity within the world.
        /// </summary>
        /// <param name="pos">The position to move the entity to.</param>
        public override void SetPosition(Location pos)
        {
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

        /// <summary>
        /// Gets the binary save data for a generic physics entity, used as part of the save procedure for a physics entity.
        /// Returns 76 bytes currently.
        /// </summary>
        /// <returns>The binary data.</returns>
        public byte[] GetPhysicsBytes()
        {
            byte[] bytes = new byte[12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4];
            GetPosition().ToBytes().CopyTo(bytes, 0);
            GetVelocity().ToBytes().CopyTo(bytes, 12);
            GetAngularVelocity().ToBytes().CopyTo(bytes, 12 + 12);
            Quaternion quat = GetOrientation();
            Utilities.FloatToBytes(quat.X).CopyTo(bytes, 12 + 12 + 12);
            Utilities.FloatToBytes(quat.Y).CopyTo(bytes, 12 + 12 + 12 + 4);
            Utilities.FloatToBytes(quat.Z).CopyTo(bytes, 12 + 12 + 12 + 4 + 4);
            Utilities.FloatToBytes(quat.W).CopyTo(bytes, 12 + 12 + 12 + 4 + 4 + 4);
            int p = 12 + 12 + 12 + 4 + 4 + 4 + 4;
            GetGravity().ToBytes().CopyTo(bytes, p);
            Utilities.FloatToBytes(GetBounciness()).CopyTo(bytes, p + 12);
            Utilities.FloatToBytes(GetFriction()).CopyTo(bytes, p + 12 + 4);
            Utilities.FloatToBytes(GetMass()).CopyTo(bytes, p + 12 + 4 + 4);
            return bytes;
        }

        /// <summary>
        /// Applies binary save data to this entity.
        /// </summary>
        /// <param name="data">The save data.</param>
        public void ApplyBytes(byte[] data)
        {
            if (data.Length < 12 + 12 + 12 + 4 + 4 + 4 + 4 + 12 + 4 + 4 + 4)
            {
                throw new Exception("Invalid binary physics entity data!");
            }
            SetPosition(Location.FromBytes(data, 0));
            SetVelocity(Location.FromBytes(data, 12));
            SetAngularVelocity(Location.FromBytes(data, 12 + 12));
            Quaternion quat = new Quaternion();
            quat.X = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 12 + 12, 4));
            quat.Y = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 12 + 12 + 4, 4));
            quat.Z = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 12 + 12 + 4 + 4, 4));
            quat.W = Utilities.BytesToFloat(Utilities.BytesPartial(data, 12 + 12 + 12 + 4 + 4 + 4, 4));
            SetOrientation(quat);
            int p = 12 + 12 + 12 + 4 + 4 + 4 + 4;
            SetGravity(Location.FromBytes(data, p));
            SetBounciness(Utilities.BytesToFloat(Utilities.BytesPartial(data, p + 12, 4)));
            SetFriction(Utilities.BytesToFloat(Utilities.BytesPartial(data, p + 12 + 4, 4)));
            SetMass(Utilities.BytesToFloat(Utilities.BytesPartial(data, p + 12 + 4 + 4, 4)));
        }
    }
}
