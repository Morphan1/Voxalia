﻿using System;
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
        public Location HalfSize = new Location(0.3f, 0.3f, 1);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;

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
            Location movement = new Location(0, 0, 0);
            if (Leftward)
            {
                movement.Y = -1;
            }
            if (Rightward)
            {
                movement.Y = 1;
            }
            if (Backward)
            {
                movement.X = 1;
            }
            if (Forward)
            {
                movement.X = -1;
            }
            bool fly = false;
            bool Slow = false;
            bool Down = false;
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.X * Utilities.PI180, fly ? Direction.Y * Utilities.PI180 : 0);
            }
            Location pvel = (movement * GetMass() * (Slow || Down ? 5 : 10)) - (fly ? Location.Zero : GetVelocity() * GetMass());
            pvel *= Slow || Down ? 5 : 10;
            if (!fly)
            {
                Body.ApplyCentralForce(new Vector3((float)pvel.X, (float)pvel.Y, 0));
                Body.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
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

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public override void Render()
        {
            // TODO: Render only when calculation shadowmap depth.
        }
    }
}
