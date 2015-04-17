using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public abstract class PhysicsEntity: Entity
    {
        public PhysicsEntity(Server tserver, bool ticks)
            : base(tserver, ticks)
        {
            Vector3 grav = TheServer.PhysicsWorld.Gravity;
            Gravity = new Location(grav.X, grav.Y, grav.Z);
        }

        /// <summary>
        /// All information on the physical version of this entity as it exists within the physics world.
        /// </summary>
        public RigidBody Body = null;

        /// <summary>
        /// The mass of the entity.
        /// </summary>
        float Mass = 0f;

        /// <summary>
        /// The friction value of the entity.
        /// </summary>
        float Friction = 0.5f;

        /// <summary>
        /// The gravity power of this entity.
        /// </summary>
        public Location Gravity;

        /// <summary>
        /// Whether this entity can rotate. Only updated at SpawnBody().
        /// </summary>
        public bool CanRotate = true;

        /// <summary>
        /// The position, rotation, etc. of this entity.
        /// </summary>
        Matrix WorldTransform = Matrix.Identity;

        /// <summary>
        /// The linear velocity of this entity.
        /// </summary>
        Location LVel = Location.Zero;

        /// <summary>
        /// The angular velocity of this entity.
        /// </summary>
        Location AVel = Location.Zero;

        /// <summary>
        /// The shape of the entity.
        /// </summary>
        public CollisionShape Shape = null;

        /// <summary>
        /// Builds and spawns the body into the world.
        /// </summary>
        public virtual void SpawnBody()
        {
            if (Body != null)
            {
                DestroyBody();
            }
            DefaultMotionState dms = new DefaultMotionState(WorldTransform);
            RigidBodyConstructionInfo rbci;
            if (Mass > 0 && CanRotate)
            {
                BulletSharp.Vector3 inertia = Shape.CalculateLocalInertia(Mass);
                rbci = new RigidBodyConstructionInfo(Mass, dms, Shape, inertia);
            }
            else
            {
                rbci = new RigidBodyConstructionInfo(Mass, dms, Shape);
            }
            rbci.Friction = Friction;
            Body = new RigidBody(rbci);
            Body.Gravity = Gravity.ToBVector();
            // TODO:  constraints
            // TODO: Does the world transform need a force-update here?
            TheServer.PhysicsWorld.AddRigidBody(Body);
        }

        /// <summary>
        /// Destroys the body, removing it from the physics world.
        /// </summary>
        public virtual void DestroyBody()
        {
            LVel = new Location(Body.LinearVelocity.X, Body.LinearVelocity.Y, Body.LinearVelocity.Z);
            AVel = new Location(Body.AngularVelocity.X, Body.AngularVelocity.Y, Body.AngularVelocity.Z);
            Gravity = new Location(Body.Gravity.X, Body.Gravity.Y, Body.Gravity.Z);
            WorldTransform = Body.WorldTransform;
            TheServer.PhysicsWorld.RemoveRigidBody(Body);
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
            return Body.Friction;
        }

        /// <summary>
        /// Sets the friction level of this entity.
        /// </summary>
        /// <param name="fric">The friction level</param>
        public virtual void SetFriction(float fric)
        {
            Friction = fric;
            if (Body != null)
            {
                Body.Friction = fric;
            }
        }

        /// <summary>
        /// Returns the mass of the entity.
        /// </summary>
        public virtual float GetMass()
        {
            return Body == null ? Mass : 1 / Body.InvMass;
        }

        /// <summary>
        /// Sets the mass of this entity.
        /// WARNING: This respawns the entity!
        /// </summary>
        /// <param name="mass">The new mass value</param>
        public virtual void SetMass(float mass)
        {
            Mass = mass;
            if (Body != null)
            {
                DestroyBody();
                SpawnBody();
            }
        }

        /// <summary>
        /// Returns the position of this entity within the world.
        /// </summary>
        public virtual Location GetPosition()
        {
            if (Body == null)
            {
                return new Location(WorldTransform.Origin.X, WorldTransform.Origin.Y, WorldTransform.Origin.Z);
            }
            Vector3 pos = Body.WorldTransform.Origin;
            return new Location(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Sets the position of this entity within the world.
        /// </summary>
        /// <param name="pos">The position to move the entity to</param>
        public virtual void SetPosition(Location pos)
        {
            if (Body != null)
            {
                Body.Translate((pos - GetPosition()).ToBVector());
            }
            else
            {
                WorldTransform.Origin = pos.ToBVector();
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
        /// <param name="vel">The new velocity</param>
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
        /// <param name="vel">The new velocity</param>
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
        /// <returns></returns>
        public OpenTK.Matrix4 GetTransformationMatrix()
        {
            Matrix mat = Body.WorldTransform;
            return new OpenTK.Matrix4(mat.M11, mat.M12, mat.M13, mat.M14, mat.M21, mat.M22, mat.M23,
                mat.M24, mat.M31, mat.M32, mat.M33, mat.M34, mat.M41, mat.M42, mat.M43, mat.M44);
        }

        /// <summary>
        /// Returns the yaw/pitch of an entity.
        /// </summary>
        public virtual Location GetAngles()
        {
            if (Body != null)
            {
                WorldTransform = Body.WorldTransform;
            }
            Location rot;
            rot.X = Math.Atan2(WorldTransform.M32, WorldTransform.M33) * 180 / Math.PI;
            rot.Y = -Math.Asin(WorldTransform.M31) * 180 / Math.PI;
            rot.Z = Math.Atan2(WorldTransform.M21, WorldTransform.M11) * 180 / Math.PI;
            return rot;
        }

        /// <summary>
        /// Sets the direction of the entity.
        /// </summary>
        /// <param name="rot">The new angles</param>
        public virtual void SetAngles(Location rot)
        {
            if (Body != null)
            {
                WorldTransform = Body.WorldTransform;
            }
            Matrix SpawnMatrix = Matrix.RotationX((float)(rot.X * Utilities.PI180));
            SpawnMatrix *= Matrix.RotationY((float)(rot.Y * Utilities.PI180));
            SpawnMatrix *= Matrix.RotationZ((float)(rot.Z * Utilities.PI180));
            SpawnMatrix *= Matrix.Translation(WorldTransform.Origin);
            WorldTransform = SpawnMatrix;
            if (Body != null)
            {
                Body.WorldTransform = SpawnMatrix;
            }
        }
    }
}
