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
    class OtherPlayerEntity : PhysicsEntity, EntityAnimated
    {
        public SingleAnimation hAnim;
        public SingleAnimation tAnim;
        public SingleAnimation lAnim;

        public bool IsFlying = false;
        public float PreFlyMass = 0;

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

        public Location Direction = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Walk = false;
        
        public float tmass = 70;

        public Model model;

        bool pup = false;
        
        public OtherPlayerEntity(Region tregion)
            : base (tregion, true, true)
        {
            SetMass(tmass);
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

        public double SoundTimeout = 0;

        public void PlayRelevantSounds()
        {
            if (SoundTimeout > 0)
            {
                SoundTimeout -= TheRegion.Delta;
                return;
            }
            if (GetVelocity().LengthSquared() < 0.2)
            {
                return;
            }
            Material mat = TheRegion.GetBlockMaterial(GetPosition() + new Location(0, 0, -0.05f));
            MaterialSound sound = mat.Sound();
            if (sound == MaterialSound.NONE)
            {
                return;
            }
            new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(GetPosition(), sound, 1f, 0.14f * (float)GetVelocity().Length());
            SoundTimeout = (Utilities.UtilRandom.NextDouble() * 0.2 + 1.0) / GetVelocity().Length();
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
            float speedmod = 1f;
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
            if (IsFlying)
            {
                Location forw = Utilities.RotateVector(new Location(-movement.Y, movement.X, 0), Direction.Yaw * Utilities.PI180, Direction.Pitch * Utilities.PI180);
                SetPosition(GetPosition() + forw * TheRegion.Delta * CBStandSpeed * 2 * (Upward ? 2 : 1));
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
            CBody = new CharacterController(WorldTransform.Translation, CBHHeight * 2f, CBHHeight * 1.1f,
                CBHHeight * 1f, CBRadius, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
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

        public float CBHHeight = 1.3f;

        public float CBProneSpeed = 1f;

        public float CBMargin = 0.01f;

        public float CBStepHeight = 0.6f;

        public float CBDownStepHeight = 0.6f;

        public float CBRadius = 0.75f;

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
            return GetPosition() + new Location(0, 0, CBHHeight * 1.8f);
        }

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public override Location GetPosition()
        {
            if (Body != null)
            {
                RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
                BoundingBox box;
                Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
                return base.GetPosition() + new Location(0, 0, box.Min.Z);
            }
            return base.GetPosition() - new Location(0, 0, CBHHeight);
        }

        public override void SetPosition(Location pos)
        {
            if (Body != null)
            {
                RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
                BoundingBox box;
                Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
                base.SetPosition(pos + new Location(0, 0, -box.Min.Z));
            }
            else
            {
                base.SetPosition(pos + new Location(0, 0, CBHHeight));
            }
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
                if (!TheClient.RenderingShadows)
                {
                    TheClient.Rendering.SetReflectionAmt(0f);
                }
            }
        }
    }
}
