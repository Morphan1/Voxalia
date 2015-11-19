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
        public PhysicsEntity SeatHolder;
        public Location PositionOffset;
        public PhysicsEntity Sitter = null;
        public Location OldPosition = Location.Zero;

        private JointWeld jw = null;

        public Seat(PhysicsEntity seatHolder, Location posOffset)
        {
            SeatHolder = seatHolder;
            PositionOffset = posOffset;
        }

        public bool Accept(PhysicsEntity sitter)
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
            jw = new JointWeld(SeatHolder, Sitter);
            SeatHolder.TheRegion.AddJoint(jw);
            return true;
        }

        public void Kick()
        {
            if (jw == null)
            {
                return;
            }
            SeatHolder.TheRegion.DestroyJoint(jw);
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
            jw = null;
        }
    }
}
