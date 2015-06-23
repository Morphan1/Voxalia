using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.JointSystem;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public OpenTK.Graphics.Color4 Color;

        public Entity(World tworld, bool tickme, bool cast_shadows)
        {
            TheWorld = tworld;
            TheClient = tworld.TheClient;
            Ticks = tickme;
            Color = new OpenTK.Graphics.Color4((float)Utilities.UtilRandom.NextDouble(), (float)Utilities.UtilRandom.NextDouble(), 0f, 1f);
            CastShadows = cast_shadows;
        }

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public long EID;

        /// <summary>
        /// Whether this entity should tick.
        /// </summary>
        public readonly bool Ticks;

        /// <summary>
        /// Wether this entity should cast shadows.
        /// </summary>
        public readonly bool CastShadows;

        /// <summary>
        /// The client that manages this entity.
        /// </summary>
        public Client TheClient = null;

        public World TheWorld = null;

        /// <summary>
        /// Draw the entity in the 3D world.
        /// </summary>
        public abstract void Render();

        public bool Visible = false;

        public abstract BEPUutilities.Quaternion GetOrientation();

        public abstract void SetOrientation(BEPUutilities.Quaternion quat);

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
    }
}
