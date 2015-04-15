using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using BulletSharp;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class PlayerEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(0.3f, 0.3f, 1);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Downward = false;

        bool pup = false;

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
            bool fly = false;
            bool on_ground = TheClient.Collision.CuboidLineIsSolid(new Location(0.2f, 0.2f, 0.1f), GetPosition() + new Location(0, 0, 0.1f), GetPosition());
            if (Upward && !fly && !pup && on_ground)
            {
                Body.ApplyCentralForce((Location.UnitZ * GetMass() * 500).ToBVector()); // Why is so much force needed to lift us off the ground?
                Body.Activate();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
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
            bool Slow = false;
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.X * Utilities.PI180, fly ? Direction.Y * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * GetMass() * MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity() * GetMass());
            if (pvel.LengthSquared() > GetMass() * GetMass() * 2)
            {
                pvel = pvel.Normalize() * GetMass() * 2;
            }
            pvel *= MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            if (!fly && on_ground)
            {
                Body.ApplyCentralForce(new Vector3((float)pvel.X, (float)pvel.Y, 0));
                Body.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            KeysPacketData kpd = (Forward ? KeysPacketData.FORWARD : 0) | (Backward ? KeysPacketData.BACKWARD : 0)
                 | (Leftward ? KeysPacketData.LEFTWARD : 0) | (Rightward ? KeysPacketData.RIGHTWARD : 0)
                  | (Upward ? KeysPacketData.UPWARD : 0) | (Downward ? KeysPacketData.DOWNWARD : 0);
            TheClient.Network.SendPacket(new KeysPacketOut(kpd, Direction));
        }

        public float MoveSpeed = 10;

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
            // TODO: Render only when calculating shadowmap depth.
        }
    }
}
