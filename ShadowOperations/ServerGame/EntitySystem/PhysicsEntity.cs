using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using BEPUutilities;
using BEPUphysics;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public abstract class PhysicsEntity: Entity
    {
        public PhysicsEntity(Server tserver, bool ticks)
            : base(tserver, ticks)
        {
            Vector3 grav = TheServer.PhysicsWorld.ForceUpdater.Gravity;
            Gravity = new Location(grav.X, grav.Y, grav.Z);
        }

        public float Widest = 1;

        /// <summary>
        /// All information on the physical version of this entity as it exists within the physics world.
        /// </summary>
        public BEPUphysics.Entities.Entity Body = null;

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
        public BEPUphysics.Entities.Entity Shape = null;

        /// <summary>
        /// Builds and spawns the body into the world.
        /// </summary>
        public virtual void SpawnBody()
        {
            if (Body != null)
            {
                DestroyBody();
            }
            Shape.AngularVelocity = new Vector3((float)AVel.X, (float)AVel.Y, (float)AVel.Z);
            Shape.LinearVelocity = new Vector3((float)LVel.X, (float)LVel.Y, (float)LVel.Z);
            Shape.WorldTransform = WorldTransform;
            Shape.Mass = Mass;
            Shape.Tag = this;
            Shape.PositionUpdateMode = BEPUphysics.PositionUpdating.PositionUpdateMode.Continuous;
            // TODO: Other settings
            // TODO: Gravity
            // TODO: Constraints
            Body = Shape;
            SetFriction(Friction);
            TheServer.PhysicsWorld.Add(Body);
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
            TheServer.PhysicsWorld.Remove(Body);
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
        /// <param name="fric">The friction level</param>
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
        /// Returns the mass of the entity.
        /// </summary>
        public virtual float GetMass()
        {
            return Body == null ? Mass : Body.Mass;
        }

        /// <summary>
        /// Sets the mass of this entity.
        /// </summary>
        /// <param name="mass">The new mass value</param>
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
        /// <param name="pos">The position to move the entity to</param>
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
            Matrix SpawnMatrix = Matrix.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(rot.X * Utilities.PI180));
            SpawnMatrix *= Matrix.CreateFromAxisAngle(new Vector3(0, 1, 0), (float)(rot.Y * Utilities.PI180));
            SpawnMatrix *= Matrix.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(rot.Z * Utilities.PI180));
            SpawnMatrix *= Matrix.CreateTranslation(WorldTransform.Translation);
            WorldTransform = SpawnMatrix;
            if (Body != null)
            {
                Body.WorldTransform = SpawnMatrix;
            }
        }

        public override bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "velocity":
                    SetVelocity(Location.FromString(data));
                    return true;
                case "angle":
                    SetAngles(Location.FromString(data));
                    return true;
                case "angular_velocity":
                    SetAngularVelocity(Location.FromString(data));
                    return true;
                case "mass":
                    SetMass(Utilities.StringToFloat(data));
                    return true;
                case "friction":
                    SetFriction(Utilities.StringToFloat(data));
                    return true;
                default:
                    return base.ApplyVar(var, data);
            }
        }

        public override List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = base.GetVariables();
            vars.Add(new KeyValuePair<string, string>("velocity", GetVelocity().ToString()));
            vars.Add(new KeyValuePair<string, string>("angle", GetAngles().ToString()));
            vars.Add(new KeyValuePair<string, string>("angular_velocity", GetAngularVelocity().ToString()));
            vars.Add(new KeyValuePair<string, string>("mass", GetMass().ToString()));
            vars.Add(new KeyValuePair<string, string>("friction", GetFriction().ToString()));
            return vars;
        }
    }
}
