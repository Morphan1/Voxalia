using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// All information on the physical version of this entity as it exists within the physics world.
        /// </summary>
        public RigidBody Body;

        /// <summary>
        /// Draw the entity in the 3D world.
        /// </summary>
        public abstract void Render();

        /// <summary>
        /// Returns the position of this entity within the world.
        /// </summary>
        public virtual Location GetPosition()
        {
            Vector3 pos = Body.WorldTransform.Origin;
            return new Location(pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// Sets the position of this entity within the world.
        /// </summary>
        /// <param name="pos">The position to move the entity to</param>
        public virtual void SetPosition(Location pos)
        {
            Body.Translate((pos - GetPosition()).ToBVector());
        }

        /// <summary>
        /// Returns the velocity of this entity.
        /// </summary>
        public virtual Location GetVelocity()
        {
            Vector3 vel = Body.LinearVelocity;
            return new Location(vel.X, vel.Y, vel.Z);
        }

        /// <summary>
        /// Sets the velocity of this entity.
        /// </summary>
        /// <param name="vel">The new velocity</param>
        public virtual void SetVelocity(Location vel)
        {
            Body.LinearVelocity = vel.ToBVector();
        }

        /// <summary>
        /// Returns the angular velocity of this entity.
        /// </summary>
        public virtual Location GetAngularVelocity()
        {
            Vector3 vel = Body.AngularVelocity;
            return new Location(vel.X, vel.Y, vel.Z);
        }

        /// <summary>
        /// Sets the angular velocity of this entity.
        /// </summary>
        /// <param name="vel">The new velocity</param>
        public virtual void SetAngularVelocity(Location vel)
        {
            Body.AngularVelocity = vel.ToBVector();
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
    }
}
