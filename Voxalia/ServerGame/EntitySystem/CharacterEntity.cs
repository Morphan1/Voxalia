using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using BEPUphysics.Character;
using BEPUutilities;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using FreneticScript;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class CharacterEntity: LivingEntity
    {
        public CharacterEntity(Region tregion, float maxhealth)
            : base(tregion, maxhealth)
        {
        }

        public string model = "cube";

        public float mod_xrot = 0;
        public float mod_yrot = 0;
        public float mod_zrot = 0;

        public System.Drawing.Color mod_color = System.Drawing.Color.White;

        public bool Upward = false;

        public float XMove;

        public float YMove;
        
        public bool Downward = false;

        public bool Click = false;

        public bool AltClick = false;

        public bool Use = false;

        /// <summary>
        /// The direction the character is currently facing, as Yaw/Pitch.
        /// </summary>
        public Location Direction;

        public bool pup = false;

        /// <summary>
        /// Returns whether this character is currently crouching.
        /// </summary>
        public bool IsCrouching
        {
            get
            {
                return Downward || DesiredStance == Stance.Crouching || (CBody != null && CBody.StanceManager.CurrentStance == Stance.Crouching);
            }
        }

        public Location TargetPosition = Location.NaN;

        public Entity TargetEntity = null;

        public double MaxPathFindDistance = 20;

        public void GoTo(Location pos)
        {
            TargetPosition = pos;
            TargetEntity = null;
            UpdatePath();
        }

        public void GoTo(Entity e)
        {
            TargetPosition = Location.NaN;
            TargetEntity = e;
            UpdatePath();
        }

        public ListQueue<Location> Path = null;

        public float PathFindCloseEnough = 0.8f * 0.8f;

        public void UpdatePath()
        {
            Location goal = TargetPosition;
            if (goal.IsNaN())
            {
                if (TargetEntity == null)
                {
                    Path = null;
                    return;
                }
                goal = TargetEntity.GetPosition();
            }
            Location selfpos = GetPosition();
            if ((goal - selfpos).LengthSquared() > MaxPathFindDistance * MaxPathFindDistance)
            {
                TargetPosition = Location.NaN; // TODO: Configurable "can't find path" result -> giveup vs. teleport
                TargetEntity = null;
                Path = null;
                return;
            }
            List<Location> tpath = TheRegion.FindPath(selfpos, goal, MaxPathFindDistance, 1);
            if (tpath == null)
            {
                TargetPosition = Location.NaN; // TODO: Configurable "can't find path" result -> giveup vs. teleport
                TargetEntity = null;
                Path = null;
                return;
            }
            Path = new ListQueue<Location>(tpath);
            PathUpdate = 1; // TODO: Configurable update time
        }

        /// <summary>
        /// The internal physics character body.
        /// </summary>
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

        public override AbstractPacketOut GetUpdatePacket()
        {
            return new CharacterUpdatePacketOut(this);
        }

        public override AbstractPacketOut GetSpawnPacket()
        {
            return new SpawnCharacterPacketOut(this);
        }

        public double PathUpdate = 0;

        bool PathMovement = false;

        public override void Tick()
        {
            if (TheRegion.Delta <= 0)
            {
                return;
            }
            if (!IsSpawned)
            {
                return;
            }
            PathUpdate -= TheRegion.Delta;
            if (PathUpdate <= 0)
            {
                UpdatePath();
            }
            if (Path != null)
            {
                PathMovement = true;
                Location spos = GetPosition();
                while (Path.Length > 0 && ((Path.Peek() - spos).LengthSquared() < PathFindCloseEnough || (Path.Peek() - spos + new Location(0, 0, -1.5)).LengthSquared() < PathFindCloseEnough))
                {
                    Path.Pop();
                }
                if (Path.Length <= 0)
                {
                    Path = null;
                }
                else
                {
                    Location targetdir = (Path.Peek() - spos).Normalize();
                    Location movegoal = Utilities.RotateVector(targetdir, (270 + Direction.Yaw) * Utilities.PI180);
                    Vector2 movegoal2 = new Vector2((float)movegoal.X, (float)movegoal.Y);
                    if (movegoal2.LengthSquared() > 0)
                    {
                        movegoal2.Normalize();
                    }
                    XMove = movegoal2.X;
                    YMove = movegoal2.Y;
                    if (movegoal.Z > 0.4)
                    {
                        CBody.Jump();
                    }
                }
            }
            if (Path == null && PathMovement)
            {
                XMove = 0;
                YMove = 0;
                PathMovement = false;
            }
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
            if (Upward && !IsFlying && !pup && CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            float speedmod = new Vector2(XMove, YMove).Length() * 2;
            if (ItemDoSpeedMod)
            {
                speedmod *= ItemSpeedMod;
            }
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
            if (CurrentSeat == null)
            {
                Vector3 movement = new Vector3(XMove, YMove, 0);
                if (Upward && IsFlying)
                {
                    movement.Z = 1;
                }
                else if (Downward && IsFlying)
                {
                    movement.Z = -1;
                }
                if (movement.LengthSquared() > 0)
                {
                    movement.Normalize();
                }
                if (Downward)
                {
                    CBody.StanceManager.DesiredStance = Stance.Crouching;
                }
                else
                {
                    CBody.StanceManager.DesiredStance = DesiredStance;
                }
                CBody.HorizontalMotionConstraint.MovementDirection = new Vector2(movement.X, movement.Y);
                if (IsFlying)
                {
                    Location forw = Utilities.RotateVector(new Location(-movement.Y, movement.X, movement.Z), Direction.Yaw * Utilities.PI180, Direction.Pitch * Utilities.PI180);
                    SetPosition(GetPosition() + forw * TheRegion.Delta * CBStandSpeed * 2 * speedmod);
                    CBody.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                    Body.LinearVelocity = new Vector3(0, 0, 0);
                }
            }
            else
            {
                CurrentSeat.HandleInput(this);
            }
            base.Tick();
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

        public float ItemSpeedMod = 1;
        public bool ItemDoSpeedMod = false;

        public bool IsFlying = false;
        public float PreFlyMass = 0;

        public virtual void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            PreFlyMass = GetMass();
            IsFlying = true;
            SetMass(0);
        }

        public virtual void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            SetMass(PreFlyMass);
            IsFlying = false;
        }

        public Stance DesiredStance = Stance.Standing;

        public EntityUseable UsedNow = null;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public abstract Location GetEyePosition();

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

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override Quaternion GetOrientation()
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Direction.Pitch)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)Direction.Yaw);
        }

        public override void SetOrientation(Quaternion rot)
        {
            Matrix trot = Matrix.CreateFromQuaternion(rot);
            Location ang = Utilities.MatrixToAngles(trot);
            Direction.Yaw = ang.Yaw;
            Direction.Pitch = ang.Pitch;
        }
    }
}
