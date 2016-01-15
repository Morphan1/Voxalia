using Voxalia.Shared;
using BEPUutilities;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using BEPUphysics.Character;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;

namespace Voxalia.ClientGame.EntitySystem
{
    class OtherPlayerEntity: CharacterEntity
    {
        public void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            IsFlying = true;
            PreFlyMass = GetMass();
            SetMass(0);
        }

        public void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            IsFlying = false;
            SetMass(PreFlyMass);
        }
        
        public float tmass = 70;

        public Model model;

        bool pup = false;
        
        public OtherPlayerEntity(Region tregion)
            : base (tregion)
        {
            SetMass(tmass);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
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
            if (Upward && !IsFlying && !pup && CBody.SupportFinder.HasSupport && GetVelocity().Z < 1f)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            float speedmod = new Vector2(XMove, YMove).Length();
            Material mat = TheRegion.GetBlockMaterial(GetPosition() + new Location(0, 0, -0.05f));
            speedmod *= mat.GetSpeedMod();
            CBody.StandingSpeed = CBStandSpeed * speedmod;
            CBody.CrouchingSpeed = CBCrouchSpeed * speedmod;
            float frictionmod = 1f;
            frictionmod *= mat.GetFrictionMod();
            CBody.SlidingForce = CBSlideForce * frictionmod * Mass;
            CBody.AirForce = CBAirForce * frictionmod * Mass;
            CBody.TractionForce = CBTractionForce * frictionmod * Mass;
            CBody.VerticalMotionConstraint.MaximumGlueForce = CBGlueForce * Mass;
            Vector2 movement = new Vector2(XMove, YMove);
            if (movement.LengthSquared() > 0)
            {
                movement.Normalize();
            }
            CBody.HorizontalMotionConstraint.MovementDirection = movement;
            // TODO: Update all this movement stuff
            if (IsFlying)
            {
                Location forw = Utilities.RotateVector(new Location(-movement.Y, movement.X, 0), Direction.Yaw * Utilities.PI180, Direction.Pitch * Utilities.PI180);
                SetPosition(GetPosition() + forw * TheRegion.Delta * CBStandSpeed * 2 * speedmod);
                CBody.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                Body.LinearVelocity = new Vector3(0, 0, 0);
            }
            PlayRelevantSounds();
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

        public float MoveSpeed = 10;
        
        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            if (!TheClient.RenderingShadows)
            {
                TheClient.Rendering.SetReflectionAmt(0.7f);
            }
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateScale(1.5f)
                * OpenTK.Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180))
                * PlayerEntity.PlayerAngleMat
                * OpenTK.Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.CustomAnimationAdjustments["spine05"] = OpenTK.Matrix4.CreateRotationX(-(float)(Direction.Pitch / 2f * Utilities.PI180));
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            if (tAnim != null)
            {
                SingleAnimationNode hand = tAnim.GetNode("metacarpal2.l");
                Matrix m4 = hand.GetBoneTotalMatrix(aTTime);
                OpenTK.Matrix4 bonemat = new OpenTK.Matrix4(m4.M11, m4.M12, m4.M13, m4.M14, m4.M21, m4.M22, m4.M23, m4.M24,
                    m4.M31, m4.M32, m4.M33, m4.M34, m4.M41, m4.M42, m4.M43, m4.M44);
                OpenTK.Matrix4 nrot = OpenTK.Matrix4.CreateRotationY(180f * (float)Utilities.PI180);
                bonemat *= nrot;
                GL.UniformMatrix4(10, false, ref bonemat);
                TheClient.Models.GetModel("items/weapons/gun01").Draw(); // TODO: Grab correct model!
                bonemat = OpenTK.Matrix4.Identity;
                GL.UniformMatrix4(10, false, ref bonemat);
            }
            if (!TheClient.RenderingShadows)
            {
                TheClient.Rendering.SetReflectionAmt(0f);
            }
        }
    }
}
