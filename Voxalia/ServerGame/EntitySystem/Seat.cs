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
        public Location OldPosition = Location.Zero; // TODO: Track orientation too!
        
        private JointSlider js = null;
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
            OldPosition = Sitter.GetPosition() - SeatHolder.GetPosition();
            Sitter.SetOrientation(SeatHolder.GetOrientation());
            if (Sitter is PlayerEntity)
            {
                ((PlayerEntity)Sitter).Teleport(SeatHolder.GetPosition() + PositionOffset); // TODO: Teleport method on all entities!
            }
            else
            {
                Sitter.SetPosition(SeatHolder.GetPosition() + PositionOffset);
            }
            float len = (float)PositionOffset.Length();
            js = new JointSlider(SeatHolder, sitter, PositionOffset / len);
            jbs = new JointBallSocket(SeatHolder, sitter, sitter.GetPosition());
            SeatHolder.TheRegion.AddJoint(js);
            SeatHolder.TheRegion.AddJoint(jbs);
            if (SeatHolder is VehicleEntity && sitter is PlayerEntity)
            {
                ((VehicleEntity)SeatHolder).Accepted((PlayerEntity)sitter);
            }
            return true;
        }

        public void Kick()
        {
            if (js == null)
            {
                return;
            }
            SeatHolder.TheRegion.DestroyJoint(js);
            SeatHolder.TheRegion.DestroyJoint(jbs);
            js = null;
            jbs = null;
            if (Sitter is PlayerEntity)
            {
                ((PlayerEntity)Sitter).Teleport(OldPosition + SeatHolder.GetPosition());
            }
            else
            {
                Sitter.SetPosition(OldPosition + SeatHolder.GetPosition());
            }
            Sitter.CurrentSeat = null;
            Sitter = null;
            OldPosition = Location.Zero;
        }

        public void HandleInput(CharacterEntity player)
        {
            if (SeatHolder is CarEntity)
            {
                ((CarEntity)SeatHolder).HandleInput(player);
            }
        }
    }
}
