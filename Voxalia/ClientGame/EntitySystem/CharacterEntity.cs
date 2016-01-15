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
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;
using FreneticScript;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.JointSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public abstract class CharacterEntity : PhysicsEntity, EntityAnimated
    {
        public CharacterEntity(Region tregion)
            : base(tregion, true, true)
        {
        }
        
        public bool Upward = false;
        public bool Click = false;
        public bool AltClick = false;
        public bool Downward = false;
        public bool Use = false;

        public SingleAnimation hAnim;
        public SingleAnimation tAnim;
        public SingleAnimation lAnim;

        public double aHTime;
        public double aTTime;
        public double aLTime;

        public bool IsFlying = false;
        public float PreFlyMass = 0;

        public Stance DesiredStance = Stance.Standing;

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
        
        public float XMove = 0;

        public float YMove = 0;

        public List<JointVehicleMotor> DrivingMotors = new List<JointVehicleMotor>();

        public List<JointVehicleMotor> SteeringMotors = new List<JointVehicleMotor>();

        public void MoveVehicle()
        {
            foreach (JointVehicleMotor motor in DrivingMotors)
            {
                motor.Motor.Settings.VelocityMotor.GoalVelocity = YMove * 100;
            }
            foreach (JointVehicleMotor motor in SteeringMotors)
            {
                motor.Motor.Settings.Servo.Goal = MathHelper.Pi * -0.2f * XMove;
            }
        }

        public CharacterController CBody;

        public CharacterController GenCharCon()
        {
            // TODO: Better variable control! (Server should command every detail!)
            CharacterController cb = new CharacterController(GetPosition().ToBVector(), CBHHeight * 2f, CBHHeight * 1.1f,
                CBHHeight * 1f, CBRadius, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
                CBTractionForce * Mass, CBSlideSpeed, CBSlideForce * Mass, CBAirSpeed, CBAirForce * Mass, CBJumpSpeed, CBSlideJumpSpeed, CBGlueForce * Mass);
            cb.StanceManager.DesiredStance = Stance.Standing;
            cb.ViewDirection = new Vector3(1f, 0f, 0f);
            cb.Down = new Vector3(0f, 0f, -1f);
            cb.Tag = this;
            BEPUphysics.Entities.Prefabs.Cylinder tb = cb.Body;
            tb.Tag = this;
            tb.AngularDamping = 1.0f;
            tb.CollisionInformation.CollisionRules.Group = CollisionUtil.Player;
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
            ConvexEntityShape = CBody.Body.CollisionInformation.Shape;
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

        public SpotLight Flashlight = null;

        public bool IgnoreThis(BroadPhaseEntry entry) // TODO: PhysicsEntity?
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }

        public Location GetEyePosition()
        {
            Location start = GetPosition() + new Location(0, 0, CBHHeight * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8 : 1.5));
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("special06.r");
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>();
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)(Direction.Pitch / 1.75f * Utilities.PI180)));
                adjs["spine04"] = rotforw;
                Matrix m4 = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 270) * Utilities.PI180) % 360f))
                    * head.GetBoneTotalMatrix(0, adjs) * (rotforw * Matrix.CreateTranslation(new Vector3(0, 0, 0.2f)));
                m4.Transpose();
                Location end = GetPosition() + new Location(m4.Translation) * 1.5f;
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

    }
}
