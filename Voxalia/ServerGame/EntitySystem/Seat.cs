using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.JointSystem;

namespace Voxalia.ServerGame.EntitySystem
{
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
            Sitter.CurrentSeat = null;
            Sitter = null;
            OldPosition = Location.Zero;
            jfw = null;
        }
    }
}
