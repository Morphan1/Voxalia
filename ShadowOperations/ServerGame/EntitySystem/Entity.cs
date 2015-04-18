using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public Entity(Server tserver, bool tickme)
        {
            TheServer = tserver;
            Ticks = tickme;
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
        /// Whether the entity is spawned into the world.
        /// </summary>
        public bool IsSpawned = false;

        /// <summary>
        /// The server that manages this entity.
        /// </summary>
        public Server TheServer = null;

        /// <summary>
        /// Tick the entity. Default implementation throws exception.
        /// </summary>
        public virtual void Tick()
        {
            throw new NotImplementedException();
        }

        public abstract Location GetPosition();

        public abstract void SetPosition(Location pos);

        public virtual List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
            vars.Add(new KeyValuePair<string, string>("position", GetPosition().ToString()));
            return vars;
        }

        public virtual bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "position":
                    SetPosition(Location.FromString(data));
                    return true;
                default:
                    return false;
            }
        }

        public virtual void Recalculate()
        {
        }
    }
}
