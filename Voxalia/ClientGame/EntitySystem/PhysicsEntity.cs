using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using Voxalia.ClientGame.JointSystem;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using BEPUphysics;

namespace Voxalia.ClientGame.EntitySystem
{
    public abstract class PhysicsEntity: Entity
    {
        public PhysicsEntity(Region tregion, bool ticks, bool cast_shadows)
            : base(tregion, ticks, cast_shadows)
        {
            Gravity = new Location(TheRegion.PhysicsWorld.ForceUpdater.Gravity);
            CGroup = CollisionUtil.Solid;
        }

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
        internal bool CanRotate = true;

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

        public ConvexShape ConvexEntityShape = null;

        public Location InternalOffset;

        public CollisionGroup CGroup;

        bool IgnoreEverythingButWater(BroadPhaseEntry entry)
        {
            return entry.CollisionRules.Group == CollisionUtil.Water;
        }
        
        public override void Tick()
        {
            // NOTE: Do nothing here.
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
            Body.WorldTransform = WorldTransform; // TODO: Position + Quaternion
            Body.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            if (!CanRotate)
            {
                Body.AngularDamping = 1;
            }
            // TODO: Other settings
            // TODO: Gravity
            Body.Tag = this;
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
        /// Gets the transformation matrix of this entity as an OpenTK matrix.
        /// </summary>
        /// <returns>.</returns>
        public OpenTK.Matrix4 GetTransformationMatrix()
        {
            Matrix mat = Body.WorldTransform;
            return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        public OpenTK.Matrix4 GetOrientationMatrix()
        {
            if (Body == null)
            {
                return OpenTK.Matrix4.Identity;
            }
            Matrix3x3 omat = Body.OrientationMatrix;
            Matrix mat = Matrix3x3.ToMatrix4X4(omat);
            return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22,
                mat.M23, mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
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
    }
}
