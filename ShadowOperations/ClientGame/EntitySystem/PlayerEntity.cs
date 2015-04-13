using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using BulletSharp;
using ShadowOperations.ClientGame.UISystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class PlayerEntity: Entity
    {
        public Location HalfSize = new Location(1.5f, 1.5f, 3);

        public Location Direction = new Location(0, 0, 0);

        public PlayerEntity(Client tclient):
            base (tclient, true)
        {
            SetMass(100);
            Shape = new BoxShape(HalfSize.ToBVector());
            CanRotate = false;
        }

        public override void Tick()
        {
            Direction.X += MouseHandler.MouseDelta.X;
            Direction.Y += MouseHandler.MouseDelta.Y;
            while (Direction.X < 0)
            {
                Direction.X += 360;
            }
            while (Direction.X > 360)
            {
                Direction.X -= 360;
            }
            if (Direction.Y > 89.9f)
            {
                Direction.Y = 89.9f;
            }
            if (Direction.Y < -89.9f)
            {
                Direction.Y = -89.9f;
            }
        }

        public override Location GetAngles()
        {
            return Direction;
        }

        public override void SetAngles(Location rot)
        {
            Direction = rot;
        }

        public override void Render()
        {
            // TODO: Render only when calculation shadowmap depth.
        }
    }
}
