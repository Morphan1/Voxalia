//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
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
using OpenTK.Graphics.OpenGL4;
using FreneticScript;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.JointSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using BEPUphysics.Constraints.SolverGroups;

namespace Voxalia.ClientGame.EntitySystem
{
    public class PlayerEntity : CharacterEntity
    {
        public YourStatusFlags ServerFlags = YourStatusFlags.NONE;

        public Quaternion PreFlyOrient = Quaternion.Identity; // TODO: Safer default value!

        public void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            IsFlying = true;
            PreFlyMass = GetMass();
            PreFlyOrient = GetOrientation();
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            NMTWOCBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            SetMass(0);
            NMTWOCBody.Body.Mass = 0;
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            NMTWOCBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            CBody.Body.AngularVelocity = Vector3.Zero;
            NMTWOCBody.Body.AngularVelocity = Vector3.Zero;
        }

        public void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            IsFlying = false;
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            NMTWOCBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            SetMass(PreFlyMass);
            NMTWOCBody.Body.Mass = PreFlyMass;
            CBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            NMTWOCBody.Body.LocalInertiaTensorInverse = new Matrix3x3();
            CBody.Body.Orientation = PreFlyOrient;
            CBody.Body.AngularVelocity = Vector3.Zero;
            NMTWOCBody.Body.Orientation = PreFlyOrient;
            NMTWOCBody.Body.AngularVelocity = Vector3.Zero;
        }

        public Location ServerLocation = new Location(0, 0, 0);

        public float tmass = 70;

        bool pup = false;

        public Model model;

        public PlayerEntity(Region tregion)
            : base(tregion)
        {
            SetMass(tmass);
            mod_scale = 1.5f;
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
            NMTWOWorld.ForceUpdater.Gravity = TheRegion.PhysicsWorld.ForceUpdater.Gravity;
            NMTWOWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            SetPosition(new Location(0, 0, 1000));
        }

        public override ItemStack GetHeldItem()
        {
            return TheClient.GetItemForSlot(TheClient.QuickBarPos);
        }

        public bool IgnorePlayers(BroadPhaseEntry entry)
        {
            if (entry.CollisionRules.Group == CollisionUtil.Player)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
        }

        UserInputSet lUIS = null;

        public ListQueue<UserInputSet> Input = new ListQueue<UserInputSet>();

        double lPT;

        public Space NMTWOWorld = new Space(null);

        Dictionary<Vector3i, FullChunkObject> NMTWOMeshes = new Dictionary<Vector3i, FullChunkObject>();

        double lGTT = 0;

        double _gtt;
        long _id = -1;
        Location _pos;
        Location _vel;
        bool __pup;

        public void PacketFromServer(double gtt, long ID, Location pos, Location vel, bool _pup)
        {
            _gtt = gtt;
            _id = ID;
            _pos = pos;
            _vel = vel;
            __pup = _pup;
        }
        
        public void UpdateForPacketFromServer(double gtt, long ID, Location pos, Location vel, bool _pup)
        {
            ServerLocation = pos;
            if (ServerFlags.HasFlag(YourStatusFlags.INSECURE_MOVEMENT))
            {
                return;
            }
            if (InVehicle)
            {
                return;
            }
            // TODO: big solid entities!
            double now = TheRegion.GlobalTickTimeLocal;
            if (TheClient.CVars.n_movemode.ValueI == 2)
            {
                // TODO: Remove outsider chunks!
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            Vector3i ch = TheRegion.ChunkLocFor(pos) + new Vector3i(x, y, z);
                            Chunk chunk = TheRegion.GetChunk(ch);
                            if (chunk == null)
                            {
                                continue;
                            }
                            if (!NMTWOMeshes.ContainsKey(ch))
                            {
                                if (chunk.FCO != null)
                                {
                                    FullChunkObject im = new FullChunkObject(chunk.FCO.Position, chunk.FCO.ChunkShape);
                                    NMTWOWorld.Add(im);
                                    NMTWOMeshes[ch] = im;
                                }
                            }
                        }
                    }
                }
                AddUIS();
                int xf = 0;
                double jumpback = gtt - lGTT;
                if (jumpback < 0)
                {
                    return;
                }
                double target = TheRegion.GlobalTickTimeLocal - jumpback;
                UserInputSet past = null;
                while (xf < Input.Length)
                {
                    UserInputSet uis = Input[xf];
                    if (uis.GlobalTimeLocal < target)
                    {
                        past = uis;
                        Input.Pop();
                        continue;
                    }
                    else if (xf == 0)
                    {
                        double mult = Math.Max(Math.Min(jumpback / TheClient.CVars.n_movement_adjustment.ValueD, 1.0), 0.01);
                        NMTWOSetPosition(uis.Position + (pos - uis.Position) * mult);
                        NMTWOSetVelocity(uis.Velocity + (vel - uis.Velocity) * mult);
                    }
                    xf++;
                    double delta;
                    if (xf < 2)
                    {
                        if (past == null)
                        {
                            continue;
                        }
                        delta = uis.GlobalTimeLocal - target;
                        SetBodyMovement(NMTWOCBody, past);
                    }
                    else
                    {
                        UserInputSet prev = Input[xf - 2];
                        delta = uis.GlobalTimeLocal - prev.GlobalTimeLocal;
                        SetBodyMovement(NMTWOCBody, prev);
                    }
                    SetMoveSpeed(NMTWOCBody, uis);
                    if (!_pup)
                    {
                        NMTWOTryToJump(uis);
                    }
                    lPT = uis.GlobalTimeLocal;
                    NMTWOWorld.Update((float)delta);
                    FlyForth(NMTWOCBody, uis, delta); // TODO: Entirely disregard NWTWOWorld if flying?
                }
                AddUIS();
                SetPosition(NMTWOGetPosition());
                SetVelocity(new Location(NMTWOCBody.Body.LinearVelocity));
                pup = _pup;
                lGTT = gtt;
            }
            else
            {
                double delta = lPT - now;
                Location dir = pos - TheClient.Player.GetPosition();
                if (dir.LengthSquared() < TheClient.CVars.n_movement_maxdistance.ValueF * TheClient.CVars.n_movement_maxdistance.ValueF)
                {
                    SetPosition(GetPosition() + dir / Math.Max(TheClient.CVars.n_movement_adjustment.ValueF / delta, 1));
                    Location veldir = vel - GetVelocity();
                    SetVelocity(GetVelocity() + veldir / Math.Max(TheClient.CVars.n_movement_adjustment.ValueF / delta, 1));
                }
                else
                {
                    TheClient.Player.SetPosition(pos);
                    TheClient.Player.SetVelocity(vel);
                }
                lPT = now;
            }
        }

        long cPacketID = 0;

        public bool InVehicle = false;

        public Entity Vehicle = null;

        public void AddUIS()
        {
            UserInputSet uis = new UserInputSet()
            {
                ID = cPacketID++,
                Upward = Upward,
                XMove = XMove,
                YMove = YMove,
                Direction = Direction,
                Position = GetPosition(),
                Velocity = GetVelocity(),
                Downward = Downward,
                GlobalTimeRemote = lGTT,
                pup = pup,
                SprintOrWalk = SprintOrWalk,
                GlobalTimeLocal = TheRegion.GlobalTickTimeLocal
            };
            Input.Push(uis);
            lUIS = uis;
        }

        public void UpdateLocalMovement()
        {
            AddUIS();
            KeysPacketData kpd = (Upward ? KeysPacketData.UPWARD : 0)
                  | (Click ? KeysPacketData.CLICK : 0) | (AltClick ? KeysPacketData.ALTCLICK : 0)
                  | (Downward ? KeysPacketData.DOWNWARD : 0)
                  | (Use ? KeysPacketData.USE : 0)
                  | (ItemLeft ? KeysPacketData.ITEMLEFT : 0)
                  | (ItemRight ? KeysPacketData.ITEMRIGHT : 0)
                  | (ItemUp ? KeysPacketData.ITEMUP : 0)
                  | (ItemDown ? KeysPacketData.ITEMDOWN : 0);
            if (ServerFlags.HasFlag(YourStatusFlags.NO_ROTATE))
            {
                Location loc = new Location();
                loc.Yaw = tyaw;
                loc.Pitch = tpitch;
                TheClient.Network.SendPacket(new KeysPacketOut(lUIS.ID, kpd, loc, lUIS.XMove, lUIS.YMove, GetPosition(), GetVelocity(), lUIS.SprintOrWalk));
            }
            else
            {
                TheClient.Network.SendPacket(new KeysPacketOut(lUIS.ID, kpd, Direction, lUIS.XMove, lUIS.YMove, GetPosition(), GetVelocity(), lUIS.SprintOrWalk));
            }
        }

        public void SetBodyMovement(CharacterController cc, UserInputSet uis)
        {
            Vector2 movement = InVehicle ? Vector2.Zero : new Vector2(uis.XMove, uis.YMove);
            if (movement.LengthSquared() > 0)
            {
                movement.Normalize();
            }
            cc.ViewDirection = Utilities.ForwardVector_Deg(uis.Direction.Yaw, uis.Direction.Pitch).ToBVector();
            cc.HorizontalMotionConstraint.MovementDirection = movement;
            if (uis.Downward)
            {
                cc.StanceManager.DesiredStance = Stance.Crouching;
            }
            else
            {
                cc.StanceManager.DesiredStance = DesiredStance;
            }
        }

        public void FlyForth(CharacterController cc, UserInputSet uis, double delta)
        {
            if (IsFlying)
            {
                Location move = new Location(-cc.HorizontalMotionConstraint.MovementDirection.Y, cc.HorizontalMotionConstraint.MovementDirection.X, 0);
                if (uis.Upward)
                {
                    move.Z = 1;
                    move = move.Normalize();
                }
                else if (uis.Downward)
                {
                    move.Z = -1;
                    move = move.Normalize();
                }
                Location forw = Utilities.RotateVector(move, Direction.Yaw * Utilities.PI180, Direction.Pitch * Utilities.PI180);
                cc.Body.Position += (forw * delta * CBStandSpeed * 4 * (new Vector2(uis.XMove, uis.YMove).Length())).ToBVector();
                cc.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                cc.Body.LinearVelocity = new Vector3(0, 0, 0);
            }
        }

        public void TryToJump()
        {
            if (!InVehicle && Upward && !IsFlying && !pup && CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
        }

        public void NMTWOTryToJump(UserInputSet uis)
        {
            if (!InVehicle && uis.Upward && !uis.pup && !IsFlying && NMTWOCBody.SupportFinder.HasSupport)
            {
                NMTWOCBody.Jump();
                uis.pup = true;
            }
        }

        public void SetMoveSpeed(CharacterController cc, UserInputSet uis)
        {
            float speedmod = (float)new Vector2(uis.XMove, uis.YMove).Length() * 2;
            speedmod *= (1f + uis.SprintOrWalk * 0.5f);
            if (Click)
            {
                ItemStack item = TheClient.GetItemForSlot(TheClient.QuickBarPos);
                bool has = item.SharedAttributes.ContainsKey("charge");
                BooleanTag bt = has ? BooleanTag.TryFor(item.SharedAttributes["charge"]) : null;
                if (bt != null && bt.Internal && item.SharedAttributes.ContainsKey("cspeedm"))
                {
                    NumberTag nt = NumberTag.TryFor(item.SharedAttributes["cspeedm"]);
                    if (nt != null)
                    {
                        speedmod *= (float)nt.Internal;
                    }
                }
            }
            RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
            BoundingBox box;
            cc.Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            Location pos = new Location(cc.Body.Position) + new Location(0, 0, box.Min.Z);
            Material mat = TheRegion.GetBlockMaterial(pos + new Location(0, 0, -0.05f));
            speedmod *= (float)mat.GetSpeedMod();
            cc.StandingSpeed = CBStandSpeed * speedmod;
            cc.CrouchingSpeed = CBCrouchSpeed * speedmod;
            float frictionmod = 1f;
            frictionmod *= (float)mat.GetFrictionMod();
            cc.SlidingForce = CBSlideForce * frictionmod * Mass;
            cc.AirForce = CBAirForce * frictionmod * Mass;
            cc.TractionForce = CBTractionForce * frictionmod * Mass;
            cc.VerticalMotionConstraint.MaximumGlueForce = CBGlueForce * Mass;
        }

        public bool PGPJump;
        public bool PGPPrimary;
        public bool PGPSecondary;
        public bool PGPDPadLeft;
        public bool PGPDPadRight;
        public bool PGPUse;
        public bool PGPILeft;
        public bool PGPIRight;
        public bool PGPIUp;
        public bool PGPIDown;
        public bool PGPReload;

        public bool Forward;
        public bool Backward;
        public bool Leftward;
        public bool Rightward;
        public bool Sprint;
        public bool Walk;
        public bool ItemLeft;
        public bool ItemRight;
        public bool ItemUp;
        public bool ItemDown;
        double tyaw = 0;
        double tpitch = 0;

        public override void Tick()
        {
            if (!(TheClient.CScreen is GameScreen))
            {
                return;
            }
            if (CBody == null || Body == null)
            {
                return;
            }
            Body.ActivityInformation.Activate();
            if (ServerFlags.HasFlag(YourStatusFlags.NO_ROTATE))
            {
                tyaw = MouseHandler.MouseDelta.X;
                tpitch = MouseHandler.MouseDelta.Y;
                tyaw += GamePadHandler.TotalDirectionX * 90f * TheRegion.Delta;
                tpitch += GamePadHandler.TotalDirectionY * 45f * TheRegion.Delta;
            }
            else
            {
                Direction.Yaw += MouseHandler.MouseDelta.X;
                Direction.Pitch += MouseHandler.MouseDelta.Y;
                Direction.Yaw += GamePadHandler.TotalDirectionX * 90f * TheRegion.Delta;
                Direction.Pitch += GamePadHandler.TotalDirectionY * 45f * TheRegion.Delta;
            }
            Vector2 tmove;
            if (Math.Abs(GamePadHandler.TotalMovementX) > 0.05) // TODO: Threshold CVar!
            {
                tmove.X = (float)GamePadHandler.TotalMovementX;
            }
            else
            {
                tmove.X = 0;
            }
            if (Math.Abs(GamePadHandler.TotalMovementY) > 0.05)
            {
                tmove.Y = (float)GamePadHandler.TotalMovementY;
            }
            else
            {
                tmove.Y = 0;
            }
            if (tmove.LengthSquared() > 1)
            {
                tmove.Normalize();
            }
            if (tmove.X == 0 && tmove.Y == 0)
            {
                if (Forward)
                {
                    tmove.Y = 1;
                }
                if (Backward)
                {
                    tmove.Y = -1;
                }
                if (Rightward)
                {
                    tmove.X = 1;
                }
                if (Leftward)
                {
                    tmove.X = -1;
                }
                if (tmove.LengthSquared() > 1)
                {
                    tmove.Normalize();
                }
            }
            XMove = (float)tmove.X;
            YMove = (float)tmove.Y;
            if (GamePadHandler.JumpKey)
            {
                PGPJump = true;
                Upward = true;
            }
            else if (PGPJump)
            {
                Upward = false;
                PGPJump = false;
            }
            if (GamePadHandler.PrimaryKey)
            {
                PGPPrimary = true;
                Click = true;
            }
            else if (PGPPrimary)
            {
                Click = false;
                PGPPrimary = false;
            }
            if (GamePadHandler.SecondaryKey)
            {
                PGPSecondary = true;
                AltClick = true;
            }
            else if (PGPSecondary)
            {
                AltClick = false;
                PGPSecondary = false;
            }
            if (GamePadHandler.ChangeLeft)
            {
                if (!PGPDPadLeft) // TODO: Holdable?
                {
                    PGPDPadLeft = true;
                    TheClient.Commands.ExecuteCommands("itemprev"); // TODO: Less lazy!
                }
            }
            else
            {
                PGPDPadLeft = false;
            }
            if (GamePadHandler.ChangeRight)
            {
                if (!PGPDPadRight) // TODO: Holdable?
                {
                    PGPDPadRight = true;
                    TheClient.Commands.ExecuteCommands("itemnext"); // TODO: Less lazy!
                }
            }
            else
            {
                PGPDPadRight = false;
            }
            if (GamePadHandler.UseKey)
            {
                PGPUse = true;
                Use = true;
            }
            else if (PGPUse)
            {
                PGPUse = false;
                Use = false;
            }
            if (GamePadHandler.ReloadKey)
            {
                PGPReload = true;
                TheClient.Commands.ExecuteCommands("weaponreload"); // TODO: Less lazy!
            }
            else if (PGPUse)
            {
                PGPReload = false;
            }
            if (GamePadHandler.ItemLeft)
            {
                PGPILeft = true;
                ItemLeft = true;
            }
            else if (PGPILeft)
            {
                PGPILeft = false;
                ItemLeft = false;
            }
            if (GamePadHandler.ItemRight)
            {
                PGPIRight = true;
                ItemRight = true;
            }
            else if (PGPIRight)
            {
                PGPIRight = false;
                ItemRight = false;
            }
            if (GamePadHandler.ItemUp)
            {
                PGPIUp = true;
                ItemUp = true;
            }
            else if (PGPIUp)
            {
                PGPIUp = false;
                ItemUp = false;
            }
            if (GamePadHandler.ItemDown)
            {
                PGPIDown = true;
                ItemDown = true;
            }
            else if (PGPIDown)
            {
                PGPIDown = false;
                ItemDown = false;
            }
            SprintOrWalk = GamePadHandler.SprintOrWalk;
            if (Sprint)
            {
                SprintOrWalk = 1;
            }
            if (Walk)
            {
                SprintOrWalk = -1;
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
            if (TheClient.VR != null)
            {
                OpenTK.Quaternion oquat = TheClient.VR.HeadMatRot.ExtractRotation(true);
                Quaternion quat = new Quaternion(oquat.X, oquat.Y, oquat.Z, oquat.W);
                Vector3 face = -Quaternion.Transform(Vector3.UnitZ, quat);
                Direction = Utilities.VectorToAngles(new Location(face));
                //OpenTK.Vector3 headSpot = TheClient.VR.BasicHeadMat.ExtractTranslation();
                if (TheClient.VR.Left != null && TheClient.VR.Left.Trigger > 0.01f)
                {
                    OpenTK.Quaternion loquat = TheClient.VR.Left.Position.ExtractRotation(true);
                    Quaternion lquat = new Quaternion(loquat.X, loquat.Y, loquat.Z, loquat.W);
                    Vector3 lforw = -Quaternion.Transform(Vector3.UnitZ, lquat);
                    Location ldir = Utilities.VectorToAngles(new Location(lforw));
                    double goalyaw = ldir.Yaw - Direction.Yaw;
                    Vector2 resmove = new Vector2(Math.Sin(goalyaw * Utilities.PI180), Math.Cos(goalyaw * Utilities.PI180));
                    double len = resmove.Length();
                    SprintOrWalk = (float)(len * 2.0 - 1.0);
                    if (len > 1.0)
                    {
                        resmove /= len;
                    }
                    XMove = -(float)resmove.X;
                    YMove = (float)resmove.Y;
                }
            }
            TryToJump();
            UpdateLocalMovement();
            SetMoveSpeed(CBody, lUIS);
            FlyForth(CBody, lUIS, TheRegion.Delta);
            SetBodyMovement(CBody, lUIS);
            PlayRelevantSounds();
            MoveVehicle();
            if (Flashlight != null)
            {
                Flashlight.Direction = Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
                Flashlight.Reposition(GetEyePosition() + Utilities.ForwardVector_Deg(Direction.Yaw, 0) * 0.3f);
            }
            base.Tick();
            //base.SetOrientation(Quaternion.Identity);
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
            bool hasjp = HasJetpack();
            JPBoost = hasjp && ItemLeft;
            JPHover = hasjp && ItemRight;
            // TODO: Triggered by console opening/closing directly, rather than monitoring it on the tick?
            if (TheClient.Network.IsAlive)
            {
                if (UIConsole.Open && !ConsoleWasOpen)
                {
                    TheClient.Network.SendPacket(new SetStatusPacketOut(ClientStatus.TYPING, 1));
                    ConsoleWasOpen = true;
                }
                else if (!UIConsole.Open && ConsoleWasOpen)
                {
                    TheClient.Network.SendPacket(new SetStatusPacketOut(ClientStatus.TYPING, 0));
                    ConsoleWasOpen = false;
                }
            }
        }

        public bool ConsoleWasOpen = false;

        public void PostTick()
        {
            if (CBody != null && _id >= 0)
            {
                UpdateForPacketFromServer(_gtt, _id, _pos, _vel, __pup);
                _id = -1;
            }
        }

        public JointWeld Welded = null;

        public CharacterController NMTWOCBody = null;

        public override void SpawnBody()
        {
            base.SpawnBody();
            NMTWOCBody = GenCharCon();
            NMTWOWorld.Add(NMTWOCBody);
        }

        public override void DestroyBody()
        {
            base.DestroyBody();
            NMTWOWorld.Remove(NMTWOCBody);
        }

        public Quaternion GetRelativeQuaternion()
        {
            if (InVehicle && Vehicle != null && Vehicle is ModelEntity && (Vehicle as ModelEntity).Plane != null)
            {
                return Vehicle.GetOrientation();
            }
            return Quaternion.Identity;
        }

        Location NMTWOGetPosition()
        {
            RigidTransform transf = new RigidTransform(Vector3.Zero, NMTWOCBody.Body.Orientation);
            BoundingBox box;
            NMTWOCBody.Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            return new Location(NMTWOCBody.Body.Position) + new Location(0, 0, box.Min.Z);
        }

        void NMTWOSetPosition(Location pos)
        {
            RigidTransform transf = new RigidTransform(Vector3.Zero, NMTWOCBody.Body.Orientation);
            BoundingBox box;
            NMTWOCBody.Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            NMTWOCBody.Body.Position = (pos + new Location(0, 0, -box.Min.Z)).ToBVector();
        }

        void NMTWOSetVelocity(Location vel)
        {
            NMTWOCBody.Body.LinearVelocity = vel.ToBVector();
        }
        
        public float Health;

        public float MaxHealth;

        public static OpenTK.Matrix4d PlayerAngleMat = OpenTK.Matrix4d.CreateRotationZ((float)(270 * Utilities.PI180));

        public override Location GetWeldSpot()
        {
            if (Welded != null && Welded.Enabled)
            {
                RigidTransform relative;
                RigidTransform start;
                if (Welded.Two == this)
                {
                    start = new RigidTransform(Welded.Ent1.Body.Position, Welded.Ent1.Body.Orientation);
                    RigidTransform.Invert(ref Welded.Relative, out relative);
                }
                else
                {
                    start = new RigidTransform(Welded.Ent2.Body.Position, Welded.Ent2.Body.Orientation);
                    relative = Welded.Relative;
                }
                RigidTransform res;
                RigidTransform.Multiply(ref start, ref relative, out res);
                return new Location(res.Position);
            }
            return GetPosition();
        }

        // TODO: Merge with base.Render() as much as possible!
        public override void Render()
        {
            Location renderrelpos = GetWeldSpot();
            TheClient.SetEnts();
            if (TheClient.CVars.n_debugmovement.ValueB)
            {
                TheClient.Rendering.RenderLine(ServerLocation, renderrelpos);
                TheClient.Rendering.RenderLineBox(ServerLocation + new Location(-0.2), ServerLocation + new Location(0.2));
            }
            if (TheClient.VR != null)
            {
                return;
            }
            OpenTK.Matrix4d mat = OpenTK.Matrix4d.Scale(1.5f)
                * OpenTK.Matrix4d.CreateRotationZ((Direction.Yaw * Utilities.PI180))
                * PlayerAngleMat
                * OpenTK.Matrix4d.CreateTranslation(ClientUtilities.ConvertD(renderrelpos));
            TheClient.MainWorldView.SetMatrix(2, mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.CustomAnimationAdjustments = new Dictionary<string, OpenTK.Matrix4>(SavedAdjustmentsOTK);
            // TODO: safe (no-collision) rotation check?
            model.CustomAnimationAdjustments["spine04"] = GetAdjustmentOTK("spine04") * OpenTK.Matrix4.CreateRotationX(-(float)(Direction.Pitch / 2f * Utilities.PI180));
            if (!TheClient.MainWorldView.RenderingShadows && TheClient.CVars.g_firstperson.ValueB)
            {
                model.CustomAnimationAdjustments["neck01"] = GetAdjustmentOTK("neck01") * OpenTK.Matrix4.CreateRotationX(-(float)(160f * Utilities.PI180));
            }
            else
            {
                model.CustomAnimationAdjustments["neck01"] = GetAdjustmentOTK("neck01");
            }
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            Model mod = TheClient.GetItemForSlot(TheClient.QuickBarPos).Mod;
            bool hasjp = HasJetpack();
            if (!hasjp && tAnim != null && mod != null)
            {
                mat = OpenTK.Matrix4d.CreateTranslation(ClientUtilities.ConvertD(renderrelpos));
                TheClient.MainWorldView.SetMatrix(2, mat);
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>(SavedAdjustments);
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, ((float)(Direction.Pitch / 2f * Utilities.PI180) % 360f)));
                adjs["spine04"] = GetAdjustment("spine04") * rotforw;
                SingleAnimationNode hand = tAnim.GetNode("metacarpal2.r");
                Matrix m4 = Matrix.CreateScale(1.5f, 1.5f, 1.5f)
                    * (Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 90) * Utilities.PI180) % 360f))
                    * hand.GetBoneTotalMatrix(aTTime, adjs))
                    * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-90) * Utilities.PI180) % 360f));
                OpenTK.Matrix4 bonemat = new OpenTK.Matrix4((float)m4.M11, (float)m4.M12, (float)m4.M13, (float)m4.M14,
                    (float)m4.M21, (float)m4.M22, (float)m4.M23, (float)m4.M24,
                    (float)m4.M31, (float)m4.M32, (float)m4.M33, (float)m4.M34,
                    (float)m4.M41, (float)m4.M42, (float)m4.M43, (float)m4.M44);
                GL.UniformMatrix4(40, false, ref bonemat);
                mod.LoadSkin(TheClient.Textures);
                mod.Draw();
                bonemat = OpenTK.Matrix4.Identity;
                GL.UniformMatrix4(40, false, ref bonemat);
            }
            if (hasjp)
            {
                // TODO: Abstractify!
                Model jetp = GetHeldItem().Mod;
                mat = OpenTK.Matrix4d.CreateTranslation(ClientUtilities.ConvertD(renderrelpos));
                TheClient.MainWorldView.SetMatrix(2, mat);
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>();
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, ((float)(Direction.Pitch / 2f * Utilities.PI180) % 360f)));
                adjs["spine04"] = GetAdjustment("spine04") * rotforw;
                SingleAnimationNode spine = tAnim.GetNode("spine04");
                Matrix m4 = Matrix.CreateScale(1.5f, 1.5f, 1.5f)
                    * (Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 90) * Utilities.PI180) % 360f))
                    * spine.GetBoneTotalMatrix(aTTime, adjs))
                     * Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)((90) * Utilities.PI180) % 360f));
                OpenTK.Matrix4 bonemat = new OpenTK.Matrix4((float)m4.M11, (float)m4.M12, (float)m4.M13, (float)m4.M14, (float)m4.M21, (float)m4.M22, (float)m4.M23, (float)m4.M24,
                    (float)m4.M31, (float)m4.M32, (float)m4.M33, (float)m4.M34, (float)m4.M41, (float)m4.M42, (float)m4.M43, (float)m4.M44);
                GL.UniformMatrix4(40, false, ref bonemat);
                jetp.LoadSkin(TheClient.Textures);
                jetp.Draw();
                bonemat = OpenTK.Matrix4.Identity;
                GL.UniformMatrix4(40, false, ref bonemat);
            }
            if (IsTyping)
            {
                TheClient.Textures.GetTexture("ui/game/typing").Bind(); // TODO: store!
                TheClient.Rendering.RenderBillboard(renderrelpos + new Location(0, 0, 4), new Location(2), TheClient.MainWorldView.CameraPos);
            }
        }

        public float ViewBackMod()
        {
            return (InVehicle && Vehicle != null) ? 7 : 2;
        }

        public Location GetCameraPosition()
        {
            if (TheClient.VR != null)
            {
                return GetBasicEyePos();
            }
            if (!InVehicle || Vehicle == null || TheClient.CVars.g_firstperson.ValueB)
            {
                return GetEyePosition();
            }
            Location vpos = Vehicle.GetPosition();
            return vpos;
        }
    }

    public class UserInputSet
    {
        public long ID;

        public double GlobalTimeLocal;

        public double GlobalTimeRemote;

        public Location Direction;

        public bool Upward;

        public bool pup;
        
        public float XMove;

        public float YMove;

        public bool Downward;

        public Location Position;

        public Location Velocity;

        public float SprintOrWalk;
    }
}
