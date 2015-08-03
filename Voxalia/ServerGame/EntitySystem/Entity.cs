using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.EntitySystem
{
    /// <summary>
    /// Represents an object within the world.
    /// </summary>
    public abstract class Entity
    {
        public Region TheWorld;

        public Entity(Region tworld, bool tickme)
        {
            TheWorld = tworld;
            TheServer = tworld.TheServer;
            Ticks = tickme;
        }

        public bool NetworkMe = true; // TODO: Readonly? Toggler method?

        /// <summary>
        /// The unique ID for this entity.
        /// </summary>
        public long EID;

        /// <summary>
        /// The ID used to identify this entity to joints.
        /// </summary>
        public string JointTargetID = "none";

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

        public abstract BEPUutilities.Quaternion GetOrientation();

        public abstract void SetOrientation(BEPUutilities.Quaternion quat);

        public bool Visible = true;

        public virtual List<KeyValuePair<string, string>> GetVariables()
        {
            List<KeyValuePair<string, string>> vars = new List<KeyValuePair<string, string>>();
            vars.Add(new KeyValuePair<string, string>("position", GetPosition().ToString()));
            vars.Add(new KeyValuePair<string, string>("visible", Visible ? "true" : "false"));
            vars.Add(new KeyValuePair<string, string>("jointtargetid", JointTargetID));
            return vars;
        }

        public virtual bool ApplyVar(string var, string data)
        {
            switch (var)
            {
                case "position":
                    SetPosition(Location.FromString(data));
                    return true;
                case "visible":
                    Visible = data.ToLower() == "true";
                    return true;
                case "jointtargetid":
                    JointTargetID = data;
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
