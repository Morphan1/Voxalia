﻿using Voxalia.Shared;
using BEPUutilities;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using BEPUphysics.Character;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.EntitySystem
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
                aHTime = 0;
            }
            else if (mode == 1)
            {
                tAnim = TheClient.Animations.GetAnimation(anim);
                aTTime = 0;
            }
            else
            {
                lAnim = TheClient.Animations.GetAnimation(anim);
                aLTime = 0;
            }
        }

        public Location HalfSize = new Location(0.55f, 0.55f, 1.3f);

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Walk = false;
        
        public float tmass = 100;

        public Model model;

        bool pup = false;
        
        public OtherPlayerEntity(Region tregion, Location half)
            : base (tregion, true, true)
        {
            HalfSize = half;
            SetMass(tmass / 2f);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Player)
            {
                return false;
            }
            return TheClient.TheRegion.Collision.ShouldCollide(entry);
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
            CBody.ViewDirection = Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch).ToBVector();
            bool fly = false;
            if (Upward && !fly && !pup && CBody.SupportFinder.HasSupport && GetVelocity().Z < 1f)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            Vector2 movement = new Vector2(0, 0);
            if (Leftward)
            {
                movement.X = -1;
            }
            if (Rightward)
            {
                movement.X = 1;
            }
            if (Backward)
            {
                movement.Y = -1;
            }
            if (Forward)
            {
                movement.Y = 1;
            }
            if (movement.LengthSquared() > 0)
            {
                movement.Normalize();
            }
            CBody.HorizontalMotionConstraint.MovementDirection = movement;
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
            base.Tick();
        }

        public SpotLight Flashlight = null;

        public float MoveSpeed = 10;

        public CharacterController CBody;

        public override void SpawnBody()
        {
            if (CBody != null)
            {
                DestroyBody();
            }
            // TODO: Better variable control! (Server should command every detail!)
            CBody = new CharacterController(WorldTransform.Translation, (float)HalfSize.Z * 2f, (float)HalfSize.Z * 1.1f,
                (float)HalfSize.X, CBRadius, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed,
                CBTractionForce * Mass, CBSlideSpeed, CBSlideForce * Mass, CBAirSpeed, CBAirForce * Mass, CBJumpSpeed, CBSlideJumpSpeed, CBGlueForce * Mass);
            CBody.StanceManager.DesiredStance = Stance.Standing;
            CBody.ViewDirection = new Vector3(1f, 0f, 0f);
            CBody.Down = new Vector3(0f, 0f, -1f);
            CBody.Tag = this;
            Body = CBody.Body;
            Body.Tag = this;
            Body.AngularDamping = 1.0f;
            Shape = CBody.Body.CollisionInformation.Shape;
            ConvexEntityShape = CBody.Body.CollisionInformation.Shape;
            Body.CollisionInformation.CollisionRules.Group = CollisionUtil.Player;
            CBody.StepManager.MaximumStepHeight = CBStepHeight;
            CBody.StepManager.MinimumDownStepHeight = CBDownStepHeight;
            TheRegion.PhysicsWorld.Add(CBody);
        }

        public float CBStepHeight = 0.6f;

        public float CBDownStepHeight = 0.6f;

        public float CBRadius = 0.01f;

        public float CBMaxTractionSlope = 1.0f;

        public float CBMaxSupportSlope = 1.3f;

        public float CBStandSpeed = 5.0f;

        public float CBCrouchSpeed = 2.5f;

        public float CBSlideSpeed = 3f;

        public float CBAirSpeed = 1f;

        public float CBTractionForce = 100f;

        public float CBSlideForce = 70f;

        public float CBAirForce = 50f;

        public float CBJumpSpeed = 8f;

        public float CBSlideJumpSpeed = 3.5f;

        public float CBGlueForce = 500f;

        public override void DestroyBody()
        {
            if (CBody == null)
            {
                return;
            }
            TheRegion.PhysicsWorld.Remove(CBody);
            CBody = null;
            Body = null;
        }

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
            return base.GetPosition() - new Location(0, 0, HalfSize.Z + HalfSize.X);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z + HalfSize.X));
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
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateScale(1.5f)
                * OpenTK.Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180))
                * PlayerEntity.PlayerAngleMat
                * OpenTK.Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            if (tAnim != null)
            {
                SingleAnimationNode hand = tAnim.GetNode("metacarpal2.l");
                Matrix m4 = hand.GetBoneTotalMatrix(aTTime);
                OpenTK.Matrix4 bonemat = new OpenTK.Matrix4(m4.M11, m4.M12, m4.M13, m4.M14, m4.M21, m4.M22, m4.M23, m4.M24,
                    m4.M31, m4.M32, m4.M33, m4.M34, m4.M41, m4.M42, m4.M43, m4.M44);
                OpenTK.Matrix4 nrot = OpenTK.Matrix4.CreateRotationY(180f * (float)Utilities.PI180);
                bonemat *= nrot;
                GL.UniformMatrix4(7, false, ref bonemat);
                TheClient.Models.GetModel("items/weapons/gun01.dae").Draw();
                bonemat = OpenTK.Matrix4.Identity;
                GL.UniformMatrix4(7, false, ref bonemat);
            }
        }
    }
}
