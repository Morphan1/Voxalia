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
        public Region TheRegion;

        public Entity(Region tregion, bool tickme)
        {
            TheRegion = tregion;
            TheServer = tregion.TheServer;
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
        /// The seat this entity is currently sitting in.
        /// </summary>
        public Seat CurrentSeat = null;
        
        /// <summary>
        /// The seats available to sit in on this entity.
        /// </summary>
        public List<Seat> Seats = null;

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

    public class Seat
    {
        public Entity SeatHolder;
        public Location PositionOffset;
        public Entity Sitter = null;
        public Location OldPosition = Location.Zero;

        private JointForceWeld jfw = null;

        public Seat(Entity seatHolder, Location posOffset)
        {
            SeatHolder = seatHolder;
            PositionOffset = posOffset;
        }

        public bool Accept(Entity sitter)
        {
            if (Sitter != null)
            {
                return false;
            }
            Sitter = sitter;
            if (Sitter.CurrentSeat != null)
            {
                Sitter.CurrentSeat.Kick();
            }
            Sitter.CurrentSeat = this;
            OldPosition = Sitter.GetPosition();
            if (Sitter is PlayerEntity)
            {
                ((PlayerEntity)Sitter).Teleport(SeatHolder.GetPosition() + PositionOffset);
            }
            else
            {
                Sitter.SetPosition(SeatHolder.GetPosition() + PositionOffset);
            }
            Sitter.SetOrientation(SeatHolder.GetOrientation());
            jfw = new JointForceWeld(SeatHolder, Sitter);
            SeatHolder.TheRegion.AddJoint(jfw);
            return true;
        }

        public void Kick()
        {
            SeatHolder.TheRegion.DestroyJoint(jfw);
            if (Sitter is PlayerEntity)
            {
                ((PlayerEntity)Sitter).Teleport(OldPosition);
            }
            else
            {
                Sitter.SetPosition(OldPosition);
            }
            Sitter = null;
            OldPosition = Location.Zero;
            jfw = null;
        }
    }
}
