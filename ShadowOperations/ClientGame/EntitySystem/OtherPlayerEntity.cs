using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using BEPUphysics.EntityStateManagement;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.ClientGame.GraphicsSystems;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;

namespace ShadowOperations.ClientGame.EntitySystem
{
    class OtherPlayerEntity: PhysicsEntity
    {
        public Location HalfSize = new Location(0.5f, 0.5f, 1);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Downward = false;

        public Model model;

        bool pup = false;

        public OtherPlayerEntity(Client tclient, Location half)
            : base (tclient, true, true)
        {
            HalfSize = half;
            SetMass(100);
            Shape = new Box(new BEPUutilities.Vector3(0, 0, 0), (float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)HalfSize.Z * 2f);
            Shape.AngularDamping = 1;
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human002/model.dae");
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            bool isThis = ((EntityCollidable)entry).Entity.Tag == this;
            if (isThis)
            {
                return false;
            }
            return entry.CollisionRules.Group != TheClient.Collision.NonSolid;
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
            bool on_ground = TheClient.Collision.CuboidLineTrace(new Location(HalfSize.X - 0.1f, HalfSize.Y - 0.1f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis).Hit;
            if (Upward && !fly && !pup && on_ground && GetVelocity().Z < 1f)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), (Location.UnitZ * GetMass() * 7f).ToBVector());
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
            if (!fly)
            {
                Body.ApplyImpulse(new Vector3(0, 0, 0), new Vector3((float)pvel.X, (float)pvel.Y, 0) * (on_ground ? 1f : 0.1f));
                Body.ActivityInformation.Activate();
            }
            if (fly)
            {
                SetPosition(GetPosition() + pvel / 200);
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

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            Recalculate();
        }

        public void Recalculate()
        {
        }

        public double aTime;

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateRotationZ((float)(Direction.X * Utilities.PI180))
                * OpenTK.Matrix4.CreateTranslation(GetPosition().ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            aTime += TheClient.gDelta / 10f;
            if (aTime >= 10f)
            {
                aTime -= 10f;
            }
            model.Draw(aTime);
        }
    }
}
