using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public OpenTK.Graphics.Color4 Color;

        public Entity(Client tclient, bool tickme)
        {
            TheClient = tclient;
            Ticks = tickme;
            Color = new OpenTK.Graphics.Color4((float)Utilities.UtilRandom.NextDouble(), (float)Utilities.UtilRandom.NextDouble(), 0f, 1f);
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
        /// The client that manages this entity.
        /// </summary>
        public Client TheClient = null; 

        /// <summary>
        /// Draw the entity in the 3D world.
        /// </summary>
        public abstract void Render();

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
