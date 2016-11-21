//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.Shared.Files;
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
        public CharacterEntity(Region tregion, double maxhealth)
            : base(tregion, maxhealth)
        {
            CGroup = CollisionUtil.Character;
        }

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + 200 + (Path == null ? 0 : Path.Length * 30);
        }

        public string model = "cube";

        public double mod_xrot = 0;
        public double mod_yrot = 0;
        public double mod_zrot = 0;
        public double mod_scale = 1;

        public System.Drawing.Color mod_color = System.Drawing.Color.White;

        public bool Upward = false;

        public double XMove;

        public double YMove;
        
        public bool Downward = false;

        public bool Click = false;

        public bool AltClick = false;

        public bool Use = false;

        public bool ItemLeft = false;

        public bool ItemRight = false;

        public bool ItemUp = false;

        public bool ItemDown = false;

        public double SprintOrWalk = 0f;

        public byte[] GetCharacterNetData()
        {
            DataStream ds = new DataStream();
            DataWriter dr = new DataWriter(ds);
            dr.WriteBytes(GetPosition().ToDoubleBytes());
            Quaternion quat = GetOrientation();
            dr.WriteFloat((float)quat.X);
            dr.WriteFloat((float)quat.Y);
            dr.WriteFloat((float)quat.Z);
            dr.WriteFloat((float)quat.W);
            dr.WriteFloat((float)GetMass());
            dr.WriteFloat((float)CBAirForce);
            dr.WriteFloat((float)CBAirSpeed);
            dr.WriteFloat((float)CBCrouchSpeed);
            dr.WriteFloat((float)CBDownStepHeight);
            dr.WriteFloat((float)CBGlueForce);
            dr.WriteFloat((float)CBHHeight);
            dr.WriteFloat((float)CBJumpSpeed);
            dr.WriteFloat((float)CBMargin);
            dr.WriteFloat((float)CBMaxSupportSlope);
            dr.WriteFloat((float)CBMaxTractionSlope);
            dr.WriteFloat((float)CBProneSpeed);
            dr.WriteFloat((float)CBRadius);
            dr.WriteFloat((float)CBSlideForce);
            dr.WriteFloat((float)CBSlideJumpSpeed);
            dr.WriteFloat((float)CBSlideSpeed);
            dr.WriteFloat((float)CBStandSpeed);
            dr.WriteFloat((float)CBStepHeight);
            dr.WriteFloat((float)CBTractionForce);
            dr.WriteFloat((float)mod_xrot);
            dr.WriteFloat((float)mod_yrot);
            dr.WriteFloat((float)mod_zrot);
            dr.WriteFloat((float)mod_scale);
            dr.WriteInt(mod_color.ToArgb());
            byte dtx = 0;
            if (Visible)
            {
                dtx |= 1;
            }
            if (CGroup == CollisionUtil.Solid)
            {
                dtx |= 2;
            }
            else if (CGroup == CollisionUtil.NonSolid)
            {
                dtx |= 4;
            }
            else if (CGroup == CollisionUtil.Item)
            {
                dtx |= 2 | 4;
            }
            else if (CGroup == CollisionUtil.Player)
            {
                dtx |= 8;
            }
            else if (CGroup == CollisionUtil.Water)
            {
                dtx |= 2 | 8;
            }
            else if (CGroup == CollisionUtil.WorldSolid)
            {
                dtx |= 2 | 4 | 8;
            }
            else if (CGroup == CollisionUtil.Character)
            {
                dtx |= 16;
            }
            dr.Write(dtx);
            dr.WriteInt(TheServer.Networking.Strings.IndexForString(model));
            dr.Flush();
            byte[] Data = ds.ToArray();
            dr.Close();
            return Data;
        }

        public virtual void Solidify()
        {
            CGroup = CollisionUtil.Character;
            if (Body != null)
            {
                Body.CollisionInformation.CollisionRules.Group = CGroup;
            }
        }

        public virtual void Desolidify()
        {
            CGroup = CollisionUtil.NonSolid;
            if (Body != null)
            {
                Body.CollisionInformation.CollisionRules.Group = CGroup;
            }
        }

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

        public double PathFindCloseEnough = 0.8f * 0.8f;

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
            MinZ = CBHHeight;
            if (CBody != null)
            {
                DestroyBody();
            }
            // TODO: Better variable control! (Server should command every detail!)
            CBody = new CharacterController(WorldTransform.Translation, CBHHeight * 2f * mod_scale, CBHHeight * 1.1f * mod_scale,
                CBHHeight * 1f * mod_scale, CBRadius * mod_scale, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
                CBTractionForce * Mass, CBSlideSpeed, CBSlideForce * Mass, CBAirSpeed, CBAirForce * Mass, CBJumpSpeed, CBSlideJumpSpeed, CBGlueForce * Mass);
            CBody.StanceManager.DesiredStance = Stance.Standing;
            CBody.ViewDirection = new Vector3(1f, 0f, 0f);
            CBody.Down = new Vector3(0f, 0f, -1f);
            CBody.Tag = this;
            Body = CBody.Body;
            Body.Tag = this;
            Body.AngularDamping = 1.0f;
            Shape = CBody.Body.CollisionInformation.Shape;
            Body.CollisionInformation.CollisionRules.Group = CGroup;
            CBody.StepManager.MaximumStepHeight = CBStepHeight;
            CBody.StepManager.MinimumDownStepHeight = CBDownStepHeight;
            TheRegion.PhysicsWorld.Add(CBody);
            RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
            BoundingBox box;
            Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            MinZ = box.Min.Z;
        }

        public override AbstractPacketOut GetUpdatePacket()
        {
            return new CharacterUpdatePacketOut(this);
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
                    Vector2 movegoal2 = new Vector2((double)movegoal.X, (double)movegoal.Y);
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
            double speedmod = new Vector2(XMove, YMove).Length() * 2;
            speedmod *= (1f + SprintOrWalk * 0.5f);
            if (ItemDoSpeedMod)
            {
                speedmod *= ItemSpeedMod;
            }
            Material mat = TheRegion.GetBlockMaterial(GetPosition() + new Location(0, 0, -0.05f));
            speedmod *= mat.GetSpeedMod();
            CBody.StandingSpeed = CBStandSpeed * speedmod;
            CBody.CrouchingSpeed = CBCrouchSpeed * speedmod;
            double frictionmod = 1f;
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
            RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
            BoundingBox box;
            Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            MinZ = box.Min.Z;
        }

        const double defHalfHeight = 1.3f;

        public double CBHHeight = defHalfHeight;

        public double CBProneSpeed = 1f;

        public double CBMargin = 0.01f;

        public double CBStepHeight = 0.6f;

        public double CBDownStepHeight = 0.6f;

        public double CBRadius = 0.3f;

        public double CBMaxTractionSlope = 1.0f;

        public double CBMaxSupportSlope = 1.3f;

        public double CBStandSpeed = 5.0f;

        public double CBCrouchSpeed = 2.5f;

        public double CBSlideSpeed = 3f;

        public double CBAirSpeed = 1f;

        public double CBTractionForce = 100f;

        public double CBSlideForce = 70f;

        public double CBAirForce = 50f;

        public double CBJumpSpeed = 8f;

        public double CBSlideJumpSpeed = 3.5f;

        public double CBGlueForce = 500f;

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

        public double ItemSpeedMod = 1;
        public bool ItemDoSpeedMod = false;

        public bool IsFlying = false;
        public double PreFlyMass = 0;

        public Quaternion PreFlyOrient = Quaternion.Identity; // TODO: Safer default value!

        public virtual void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            PreFlyOrient = GetOrientation();
            PreFlyMass = GetMass();
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            IsFlying = true;
            SetMass(0);
            CBody.Body.AngularVelocity = Vector3.Zero;
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
        }

        public virtual void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            SetMass(PreFlyMass);
            IsFlying = false;
            CBody.Body.Orientation = PreFlyOrient;
            CBody.Body.AngularVelocity = Vector3.Zero;
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
        }

        public Stance DesiredStance = Stance.Standing;

        public EntityUseable UsedNow = null;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public abstract Location GetEyePosition();

        public double MinZ = -defHalfHeight * 1.5f;

        public override Location GetPosition()
        {
            return base.GetPosition() + new Location(0, 0, MinZ);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, -MinZ));
        }

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override Quaternion GetOrientation()
        {
            return Quaternion.CreateFromAxisAngle(Vector3.UnitY, (double)Direction.Pitch)
                * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (double)Direction.Yaw);
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
