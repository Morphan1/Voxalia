using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.NetworkSystem.PacketsIn;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using BEPUutilities;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseSystems;
using BEPUphysics.OtherSpaceStages;
using BEPUphysics.UpdateableSystems;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.PositionUpdating;
using BEPUphysics.NarrowPhaseSystems;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SingleEntity;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;
using FreneticScript;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.JointSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ClientGame.EntitySystem
{
    public abstract class CharacterEntity : PhysicsEntity, EntityAnimated
    {
        public CharacterEntity(Region tregion)
            : base(tregion, true, true)
        {
        }

        public bool IsTyping = false;
        
        public bool Upward = false;
        public bool Click = false;
        public bool AltClick = false;
        public bool Downward = false;
        public bool Use = false;

        // TODO: This block -> HumanoidEntity?
        public SingleAnimation hAnim;
        public SingleAnimation tAnim;
        public SingleAnimation lAnim;
        public double aHTime;
        public double aTTime;
        public double aLTime;

        public bool IsFlying = false;
        public float PreFlyMass = 0;
        public bool HasFuel = false;

        public Stance DesiredStance = Stance.Standing;

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                hAnim = TheClient.Animations.GetAnimation(anim, TheClient.Files);
                aHTime = 0;
            }
            else if (mode == 1)
            {
                tAnim = TheClient.Animations.GetAnimation(anim, TheClient.Files);
                aTTime = 0;
            }
            else
            {
                lAnim = TheClient.Animations.GetAnimation(anim, TheClient.Files);
                aLTime = 0;
            }
        }

        public Location Direction = new Location(0, 0, 0);

        public double SoundTimeout = 0;

        public void PlayRelevantSounds()
        {
            if (SoundTimeout > 0)
            {
                SoundTimeout -= TheRegion.Delta;
                return;
            }
            Location vel = GetVelocity();
            if (vel.LengthSquared() < 0.2)
            {
                return;
            }
            Material mat = TheRegion.GetBlockMaterial(GetPosition() + new Location(0, 0, -0.05f));
            MaterialSound sound = mat.Sound();
            if (sound == MaterialSound.NONE)
            {
                return;
            }
            double velLen = vel.Length();
            new DefaultSoundPacketIn() { TheClient = TheClient }.PlayDefaultBlockSound(GetPosition(), sound, 1f, 0.14f * (float)velLen);
            TheClient.Particles.Steps(GetPosition(), mat, GetVelocity(), (float)velLen);
            SoundTimeout = (Utilities.UtilRandom.NextDouble() * 0.2 + 1.0) / velLen;
        }
        
        public float XMove = 0;

        public float YMove = 0;

        public float SprintOrWalk = 0f;

        public List<JointVehicleMotor> DrivingMotors = new List<JointVehicleMotor>();

        public List<JointVehicleMotor> SteeringMotors = new List<JointVehicleMotor>();

        public void MoveVehicle()
        {
            foreach (JointVehicleMotor motor in DrivingMotors)
            {
                motor.Motor.Settings.VelocityMotor.GoalVelocity = YMove * 100; // TODO: Sprint/Walk mods?
            }
            foreach (JointVehicleMotor motor in SteeringMotors)
            {
                motor.Motor.Settings.Servo.Goal = MathHelper.Pi * -0.2f * XMove;
            }
        }

        public CharacterController CBody;

        public float mod_scale = 1;

        public CharacterController GenCharCon()
        {
            // TODO: Better variable control! (Server should command every detail!)
            CharacterController cb = new CharacterController(GetPosition().ToBVector(), CBHHeight * 2f * mod_scale, CBHHeight * 1.1f * mod_scale,
                CBHHeight * 1f * mod_scale, CBRadius * mod_scale, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
                CBTractionForce * Mass, CBSlideSpeed, CBSlideForce * Mass, CBAirSpeed, CBAirForce * Mass, CBJumpSpeed, CBSlideJumpSpeed, CBGlueForce * Mass);
            cb.StanceManager.DesiredStance = Stance.Standing;
            cb.ViewDirection = new Vector3(1f, 0f, 0f);
            cb.Down = new Vector3(0f, 0f, -1f);
            cb.Tag = this;
            BEPUphysics.Entities.Prefabs.Cylinder tb = cb.Body;
            tb.Tag = this;
            tb.AngularDamping = 1.0f;
            tb.CollisionInformation.CollisionRules.Group = CGroup;
            cb.StepManager.MaximumStepHeight = CBStepHeight;
            cb.StepManager.MinimumDownStepHeight = CBDownStepHeight;
            return cb;
        }

        public override void SpawnBody()
        {
            if (CBody != null)
            {
                DestroyBody();
            }
            CBody = GenCharCon();
            Body = CBody.Body;
            Shape = CBody.Body.CollisionInformation.Shape;
            TheRegion.PhysicsWorld.Add(CBody);
            Jetpack = new JetpackMotionConstraint(this);
            TheRegion.PhysicsWorld.Add(Jetpack);
        }

        public float CBHHeight = 1.3f;

        public float CBProneSpeed = 1f;

        public float CBMargin = 0.01f;

        public float CBStepHeight = 0.6f;

        public float CBDownStepHeight = 0.6f;

        public float CBRadius = 0.3f;

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
            if (Jetpack != null)
            {
                TheRegion.PhysicsWorld.Remove(Jetpack);
                Jetpack = null;
            }
            TheRegion.PhysicsWorld.Remove(CBody);
            CBody = null;
            Body = null;
        }

        public SpotLight Flashlight = null;

        public bool IgnoreThis(BroadPhaseEntry entry) // TODO: PhysicsEntity?
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }

        public Dictionary<string, Matrix> SavedAdjustments = new Dictionary<string, Matrix>();

        public Dictionary<string, OpenTK.Matrix4> SavedAdjustmentsOTK = new Dictionary<string, OpenTK.Matrix4>();

        public void SetAdjustment(string str, Matrix matr)
        {
            SavedAdjustments[str] = matr;
            OpenTK.Matrix4 mat = ClientUtilities.Convert(matr);
            SavedAdjustmentsOTK[str] = mat;
        }

        public Matrix GetAdjustment(string str)
        {
            Matrix mat;
            if (SavedAdjustments.TryGetValue(str, out mat))
            {
                return mat;
            }
            return Matrix.Identity;
        }

        public OpenTK.Matrix4 GetAdjustmentOTK(string str)
        {
            OpenTK.Matrix4 mat;
            if (SavedAdjustmentsOTK.TryGetValue(str, out mat))
            {
                return mat;
            }
            return OpenTK.Matrix4.Identity;
        }

        public virtual Location GetWeldSpot()
        {
            return GetPosition();
        }

        public Location GetEyePosition()
        {
            Location renderrelpos = GetWeldSpot();
            Location start = renderrelpos + new Location(0, 0, CBHHeight * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8 : 1.5));
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("special06.r");
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>(SavedAdjustments);
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)(Direction.Pitch / 1.75f * Utilities.PI180)));
                adjs["spine04"] = GetAdjustment("spine04") * rotforw;
                Matrix m4 = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 270) * Utilities.PI180) % 360f))
                    * head.GetBoneTotalMatrix(0, adjs) * (rotforw * Matrix.CreateTranslation(new Vector3(0, 0, 0.2f)));
                m4.Transpose();
                Location end = renderrelpos + new Location(m4.Translation) * 1.5f;
                start.Z = end.Z; // FUTURE: Maybe handle player rotation?
                double len = (end - start).Length();
                Location normdir = (end - start) / len;
                RayCastResult rcr;
                if (TheRegion.SpecialCaseRayTrace(start, normdir, (float)len, MaterialSolidity.FULLSOLID, IgnoreThis, out rcr))
                {
                    return new Location(rcr.HitData.Location + rcr.HitData.Normal * 0.2f);
                }
                return end;
            }
            else
            {
                return start;
            }
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

        public bool JPBoost = false;
        public bool JPHover = false;

        public virtual ItemStack GetHeldItem()
        {
            // TODO: Real return
            return new ItemStack(TheClient, "air");
        }

        public bool HasChute()
        {
            return GetHeldItem().Name == "parachute";
        }

        public bool HasJetpack()
        {
            return GetHeldItem().Name == "jetpack";
        }

        public double JetpackBoostRate(out float max)
        {
            const double baseBoost = 1500.0;
            const float baseMax = 2000.0f;
            max = baseMax; // TODO: Own mod
            ItemStack its = GetHeldItem();
            TemplateObject mod;
            if (its.SharedAttributes.TryGetValue("jetpack_boostmod", out mod))
            {
                NumberTag nt = NumberTag.TryFor(mod);
                if (nt != null)
                {
                    return baseBoost * nt.Internal;
                }
            }
            return baseBoost;
        }

        public double JetpackHoverStrength()
        {
            double baseHover = GetMass();
            ItemStack its = GetHeldItem();
            TemplateObject mod;
            if (its.SharedAttributes.TryGetValue("jetpack_hovermod", out mod))
            {
                NumberTag nt = NumberTag.TryFor(mod);
                if (nt != null)
                {
                    return baseHover * nt.Internal;
                }
            }
            return baseHover;
        }

        void DoJetpackEffect(int count)
        {
            Location forw = ForwardVector();
            forw = new Location(-forw.X, -forw.Y, 0).Normalize() * 0.333;
            forw.Z = 1;
            Location pos = GetPosition() + forw;
            for (int i = 0; i < count; i++)
            {
                TheClient.Particles.JetpackEffect(pos, GetVelocity());
            }
        }

        public class JetpackMotionConstraint : SingleEntityConstraint
        {
            public CharacterEntity Character;
            public JetpackMotionConstraint(CharacterEntity character)
            {
                Character = character;
                Entity = character.Body;
            }

            public Vector3 GetMoveVector(out double glen)
            {
                Location gravity = Character.GetGravity();
                glen = gravity.Length();
                gravity /= glen;
                // TODO: Sprint/Walk mods?
                gravity += Utilities.RotateVector(new Location(Character.YMove, -Character.XMove, 0), Character.Direction.Yaw * Utilities.PI180);
                return gravity.ToBVector(); // TODO: Maybe normalize this?
            }

            public override void ExclusiveUpdate()
            {
                if (Character.HasChute())
                {
                    entity.ModifyLinearDamping(0.8f);
                }
                if (Character.HasJetpack() && Character.HasFuel)
                {
                    if (Character.JPBoost)
                    {
                        float max;
                        double boost = Character.JetpackBoostRate(out max);
                        double glen;
                        Vector3 move = GetMoveVector(out glen);
                        Vector3 vec = -(move * (float)boost) * Delta;
                        Character.CBody.Jump();
                        Entity.ApplyLinearImpulse(ref vec);
                        if (Entity.LinearVelocity.LengthSquared() > max * max)
                        {
                            Vector3 vel = entity.LinearVelocity;
                            vel.Normalize();
                            Entity.LinearVelocity = vel * max;
                        }
                        Character.DoJetpackEffect(10);
                    }
                    else if (Character.JPHover)
                    {
                        double hover = Character.JetpackHoverStrength();
                        double glen;
                        Vector3 move = GetMoveVector(out glen);
                        Vector3 vec = -(move * (float)glen * (float)hover) * Delta;
                        Entity.ApplyLinearImpulse(ref vec);
                        entity.ModifyLinearDamping(0.6f);
                        Character.DoJetpackEffect(3);
                    }
                }
            }

            float Delta;

            public override float SolveIteration()
            {
                return 0; // Do nothing
            }

            public override void Update(float dt)
            {
                Delta = dt;
            }
        }

        public JetpackMotionConstraint Jetpack = null;
    }
}
