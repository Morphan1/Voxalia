using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.ServerGame.NetworkSystem;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;

namespace ShadowOperations.ServerGame.EntitySystem
{
    public class PlayerEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(0.3f, 0.3f, 1f);

        public Connection Network;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public bool Upward = false;
        public bool Downward = false;
        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;

        public Location Direction;

        bool pup = false;

        public PhysicsEntity Grabbed = null;

        public PlayerEntity(Server tserver, Connection conn)
            : base(tserver, true)
        {
            Network = conn;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
        }

        public override void Tick()
        {
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
            TheServer.PhysicsWorld.Remove(Body);
            bool on_ground = TheServer.Collision.CuboidLineIsSolid(new Location(0.2f, 0.2f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f));
            TheServer.PhysicsWorld.Add(Body);
            if (Upward && !fly && !pup && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), (Location.UnitZ * 500f).ToBVector());
                Body.ActivityInformation.Activate();
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
            Location intent_vel = movement * MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (pvel.LengthSquared() > 4 * MoveSpeed * MoveSpeed)
            {
                pvel = pvel.Normalize() * 2 * MoveSpeed;
            }
            pvel *= MoveSpeed * (Slow || Downward ? 0.5f : 1f);
            if (!fly && on_ground)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            if (Grabbed != null)
            {
                if (Grabbed.IsSpawned && (Grabbed.GetPosition() - GetPosition()).LengthSquared() < 5 * 5 + Grabbed.Widest * Grabbed.Widest)
                {
                    Location pos = GetPosition() + new Location(0, 0, HalfSize.Z * 1.6f) + Utilities.ForwardVector_Deg(Direction.X, Direction.Y) * (2 + Grabbed.Widest);
                    Grabbed.Body.LinearVelocity = (pos - Grabbed.GetPosition()).ToBVector() * 10f;
                }
                else
                {
                    Grabbed = null;
                }
            }
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

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.6f);
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
