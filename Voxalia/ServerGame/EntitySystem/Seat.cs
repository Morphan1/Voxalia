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

        private JointBallSocket jbs = null;

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
            Sitter.SetOrientation(SeatHolder.GetOrientation());
            if (Sitter is PlayerEntity)
            {
                ((PlayerEntity)Sitter).Teleport(SeatHolder.GetPosition() + PositionOffset); // TODO: Teleport method on all entities!
                jbs = new JointBallSocket(SeatHolder, sitter, sitter.GetPosition());
            }
            else
            {
                Sitter.SetPosition(SeatHolder.GetPosition() + PositionOffset);
                jw = new JointWeld(SeatHolder, Sitter);
                SeatHolder.TheRegion.AddJoint(jw);
            }
            return true;
        }

        public void Kick()
        {
            if (jw == null && jbs == null)
            {
                return;
            }
            if (jw == null)
            {
                SeatHolder.TheRegion.DestroyJoint(jbs);
                jbs = null;
            }
            else
            {
                SeatHolder.TheRegion.DestroyJoint(jw);
                jw = null;
            }
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
        }

        public void HandleInput(PlayerEntity player)
        {
            if (SeatHolder is VehicleEntity)
            {
                ((VehicleEntity)SeatHolder).HandleInput(player);
            }
        }
    }
}
