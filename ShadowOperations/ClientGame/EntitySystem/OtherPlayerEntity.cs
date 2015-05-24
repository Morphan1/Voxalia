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
using ShadowOperations.ClientGame.GraphicsSystems.LightingSystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    class OtherPlayerEntity : PhysicsEntity, EntityAnimated
    {
        public SingleAnimation hAnim;
        public SingleAnimation tAnim;
        public SingleAnimation lAnim;

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                hAnim = TheClient.Animations.GetAnimation(anim);
            }
            else if (mode == 1)
            {
                tAnim = TheClient.Animations.GetAnimation(anim);
            }
            else
            {
                lAnim = TheClient.Animations.GetAnimation(anim);
            }
        }

        public Location HalfSize = new Location(0.5f, 0.5f, 0.9f);

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
            model = TheClient.Models.GetModel("players/human_male_004.dae");
            model.LoadSkin(tclient.Textures);
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
            while (Direction.Yaw < 0)
            {
                Direction.Yaw += 360;
            }
            while (Direction.Yaw > 360)
            {
                Direction.Yaw -= 360;
            }
            if (Direction.Pitch > 89.9f)
            {
                Direction.Pitch = 89.9f;
            }
            if (Direction.Pitch < -89.9f)
            {
                Direction.Pitch = -89.9f;
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
                movement = Utilities.RotateVector(movement, Direction.Yaw * Utilities.PI180, fly ? Direction.Pitch * Utilities.PI180 : 0).Normalize();
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
            /*
            if (!Utilities.IsCloseTo((float)base.GetAngles().Z, 0, 1))
            {
                base.SetAngles(new Location(0, 0, 0));
            }*/ // See server.player
            if (Flashlight != null)
            {
                Flashlight.Direction = Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
                Flashlight.Reposition(GetEyePosition() + Flashlight.Direction * 0.3f);
            }
            aHTime += TheClient.Delta;
            aTTime += TheClient.Delta;
            aLTime += TheClient.Delta;
            if (hAnim != null)
            {
                if (aHTime >= hAnim.Length)
                {
                    aHTime = 0;
                }
            }
            if (tAnim != null)
            {
                if (aTTime >= tAnim.Length)
                {
                    aTTime = 0;
                }
            }
            if (lAnim != null)
            {
                if (aLTime >= lAnim.Length)
                {
                    aLTime = 0;
                }
            }
        }

        public SpotLight Flashlight = null;

        public float MoveSpeed = 10;

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8f);
        }

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
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
            Body.CollisionInformation.CollisionRules.Group = TheClient.Collision.Player;
        }

        public double aHTime;
        public double aTTime;
        public double aLTime;

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180))
                * OpenTK.Matrix4.CreateTranslation(GetPosition().ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            if (tAnim != null)
            {
                SingleAnimationNode hand = tAnim.GetNode("hand.l");
                Matrix m4 = hand.GetBoneTotalMatrix(/*model.OriginalModel, */aTTime);
                OpenTK.Matrix4 bonemat = /*new OpenTK.Matrix4(m4.M11, m4.M12, m4.M13, m4.M14, m4.M21, m4.M22, m4.M23, m4.M24,
                    m4.M31, m4.M32, m4.M33, m4.M34, m4.M41, m4.M42, m4.M43, m4.M44)
                    * */OpenTK.Matrix4.CreateTranslation(-0.4968f, -0.2802f, 0.99510f);
                mat = bonemat * mat;
                GL.UniformMatrix4(2, false, ref mat);
                //GL.UniformMatrix4(6, false, ref bonemat);
                TheClient.Models.Cube.Draw();
                bonemat = OpenTK.Matrix4.Identity;
                //GL.UniformMatrix4(6, false, ref bonemat);
            }
        }
    }
}
