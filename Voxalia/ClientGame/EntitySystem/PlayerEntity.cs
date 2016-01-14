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

namespace Voxalia.ClientGame.EntitySystem
{
    public class PlayerEntity : CharacterEntity
    {
        public YourStatusFlags ServerFlags = YourStatusFlags.NONE;

        public void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            IsFlying = true;
            PreFlyMass = GetMass();
            SetMass(0);
            NMTWOCBody.Body.Mass = 0;
        }

        public void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            IsFlying = false;
            SetMass(PreFlyMass);
            NMTWOCBody.Body.Mass = PreFlyMass;
        }

        public Location ServerLocation = new Location(0, 0, 0);

        public float tmass = 70;

        bool pup = false;

        public Model model;

        public PlayerEntity(Region tregion)
            : base(tregion)
        {
            SetMass(tmass);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
            NMTWOWorld.ForceUpdater.Gravity = TheRegion.PhysicsWorld.ForceUpdater.Gravity;
            NMTWOWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            SetPosition(new Location(0, 0, 1000));
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

        Dictionary<Location, FullChunkObject> NMTWOMeshes = new Dictionary<Location, FullChunkObject>();

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

        HashSet<Location> Quiet = new HashSet<Location>();

        public void UpdateForPacketFromServer(double gtt, long ID, Location pos, Location vel, bool _pup)
        {
            double now = TheRegion.GlobalTickTimeLocal;
            ServerLocation = pos;
            if (TheClient.CVars.n_movemode.ValueI == 2)
            {
                // TODO: Remove outsider chunks!
                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        for (int z = -1; z <= 1; z++)
                        {
                            Location ch = TheRegion.ChunkLocFor(pos) + new Location(x, y, z);
                            Chunk chunk = TheRegion.GetChunk(ch);
                            if (chunk == null)
                            {
                                if (!Quiet.Contains(ch))
                                {
                                    Quiet.Add(ch);
                                    SysConsole.Output(OutputType.WARNING, "Moving around in non-loaded chunks! For loc: " + ch);
                                }
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

        public void AddUIS()
        {
            UserInputSet uis = new UserInputSet()
            {
                ID = cPacketID++,
                Upward = Upward,
                Sprint = Sprint,
                Walk = Walk,
                Forward = Forward,
                Backward = Backward,
                Leftward = Leftward,
                Rightward = Rightward,
                Direction = Direction,
                Position = GetPosition(),
                Velocity = GetVelocity(),
                Downward = Downward,
                GlobalTimeRemote = lGTT,
                pup = pup,
                GlobalTimeLocal = TheRegion.GlobalTickTimeLocal
            };
            Input.Push(uis);
            lUIS = uis;
        }

        public void UpdateLocalMovement()
        {
            AddUIS();
            KeysPacketData kpd = (Forward ? KeysPacketData.FORWARD : 0) | (Backward ? KeysPacketData.BACKWARD : 0)
                 | (Leftward ? KeysPacketData.LEFTWARD : 0) | (Rightward ? KeysPacketData.RIGHTWARD : 0)
                 | (Upward ? KeysPacketData.UPWARD : 0) | (Walk ? KeysPacketData.WALK : 0)
                  | (Click ? KeysPacketData.CLICK : 0) | (AltClick ? KeysPacketData.ALTCLICK : 0)
                  | (Sprint ? KeysPacketData.SPRINT : 0) | (Downward ? KeysPacketData.DOWNWARD : 0)
                  | (Use ? KeysPacketData.USE : 0);
            TheClient.Network.SendPacket(new KeysPacketOut(lUIS.ID, kpd, Direction));
        }

        public void SetBodyMovement(CharacterController cc, UserInputSet uis)
        {
            Vector2 movement = new Vector2(0, 0);
            if (uis.Leftward)
            {
                movement.X = -1;
            }
            if (uis.Rightward)
            {
                movement.X = 1;
            }
            if (uis.Backward)
            {
                movement.Y = -1;
            }
            if (uis.Forward)
            {
                movement.Y = 1;
            }
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
                cc.Body.Position += (forw * delta * CBStandSpeed * 2 * (uis.Sprint ? 2 : (uis.Walk ? 0.5 : 1))).ToBVector();
                cc.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                cc.Body.LinearVelocity = new Vector3(0, 0, 0);
            }
        }

        public void TryToJump()
        {
            if (Upward && !IsFlying && !pup && CBody.SupportFinder.HasSupport)
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
            if (uis.Upward && !uis.pup && !IsFlying && NMTWOCBody.SupportFinder.HasSupport)
            {
                NMTWOCBody.Jump();
                uis.pup = true;
            }
        }

        public void SetMoveSpeed(CharacterController cc, UserInputSet uis)
        {
            float speedmod = uis.Sprint ? 1.5f : (uis.Walk ? 0.5f : 1f);
            if (Click)
            {
                ItemStack item = TheClient.GetItemForSlot(TheClient.QuickBarPos);
                if (item.SharedAttributes.ContainsKey("charge") && item.SharedAttributes["charge"] == 1f)
                {
                    speedmod *= item.SharedAttributes["cspeedm"];
                }
            }
            RigidTransform transf = new RigidTransform(Vector3.Zero, Body.Orientation);
            BoundingBox box;
            cc.Body.CollisionInformation.Shape.GetBoundingBox(ref transf, out box);
            Location pos = new Location(cc.Body.Position) + new Location(0, 0, box.Min.Z);
            Material mat = TheRegion.GetBlockMaterial(pos + new Location(0, 0, -0.05f));
            speedmod *= mat.GetSpeedMod();
            cc.StandingSpeed = CBStandSpeed * speedmod;
            cc.CrouchingSpeed = CBCrouchSpeed * speedmod;
            float frictionmod = 1f;
            frictionmod *= mat.GetFrictionMod();
            cc.SlidingForce = CBSlideForce * frictionmod * Mass;
            cc.AirForce = CBAirForce * frictionmod * Mass;
            cc.TractionForce = CBTractionForce * frictionmod * Mass;
            cc.VerticalMotionConstraint.MaximumGlueForce = CBGlueForce * Mass;
        }

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
            Direction.Yaw += MouseHandler.MouseDelta.X;
            Direction.Pitch += MouseHandler.MouseDelta.Y;
            Direction.Yaw += GamePadHandler.TotalDirectionX * 90f * TheRegion.Delta;
            Direction.Pitch += GamePadHandler.TotalDirectionY * 45f * TheRegion.Delta;
            if (GamePadHandler.TotalMovementX > 0.5)
            {
                PGPRight = true;
                Rightward = true;
            }
            else if (PGPRight)
            {
                Rightward = false;
                PGPRight = false;
            }
            if (GamePadHandler.TotalMovementY > 0.5)
            {
                PGPUp = true;
                Forward = true;
            }
            else if (PGPUp)
            {
                Forward = false;
                PGPUp = false;
            }
            if (GamePadHandler.TotalMovementX < -0.5)
            {
                PGPLeft = true;
                Leftward = true;
            }
            else if (PGPLeft)
            {
                Leftward = false;
                PGPLeft = false;
            }
            if (GamePadHandler.TotalMovementY < -0.5)
            {
                PGPDown = true;
                Backward = true;
            }
            else if (PGPDown)
            {
                Backward = false;
                PGPDown = false;
            }
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
            if (GamePadHandler.DPadLeft)
            {
                if (!PGPDPadLeft)
                {
                    PGPDPadLeft = true;
                    TheClient.Commands.ExecuteCommands("itemprev"); // TODO: Less lazy!
                }
            }
            else
            {
                PGPDPadLeft = false;
            }
            if (GamePadHandler.DPadRight)
            {
                if (!PGPDPadRight)
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
                TheClient.Commands.ExecuteCommands("use");
                //TODO: Use = true;
            }
            else if (PGPUse)
            {
                PGPUse = false;
                //TODO: Use = false;
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
        }

        public void PostTick()
        {
            if (CBody != null && _id >= 0)
            {
                UpdateForPacketFromServer(_gtt, _id, _pos, _vel, __pup);
                _id = -1;
            }
        }

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

        public static OpenTK.Matrix4 PlayerAngleMat = OpenTK.Matrix4.CreateRotationZ((float)(270 * Utilities.PI180));

        public override void Render()
        {
            if (TheClient.CVars.n_debugmovement.ValueB)
            {
                TheClient.Rendering.RenderLine(ServerLocation, GetPosition());
                TheClient.Rendering.RenderLineBox(ServerLocation + new Location(-0.2), ServerLocation + new Location(0.2));
            }
            if (!TheClient.RenderingShadows)
            {
                TheClient.Rendering.SetReflectionAmt(0.7f);
            }
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateScale(1.5f)
                * OpenTK.Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180))
                * PlayerAngleMat
                * OpenTK.Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            // TODO: safe (no-collision) rotation check?
            model.CustomAnimationAdjustments["spine04"] = OpenTK.Matrix4.CreateRotationX(-(float)(Direction.Pitch / 2f * Utilities.PI180));
            if (!TheClient.RenderingShadows && TheClient.CVars.g_firstperson.ValueB)
            {
                model.CustomAnimationAdjustments["neck01"] = OpenTK.Matrix4.CreateRotationX(-(float)(160f * Utilities.PI180));
            }
            else
            {
                model.CustomAnimationAdjustments["neck01"] = OpenTK.Matrix4.Identity;
            }
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            if (!TheClient.RenderingShadows)
            {
                TheClient.Rendering.SetReflectionAmt(0f);
            }
            Model mod = TheClient.GetItemForSlot(TheClient.QuickBarPos).Mod;
            if (tAnim != null && mod != null)
            {
                mat = OpenTK.Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
                GL.UniformMatrix4(2, false, ref mat);
                Dictionary<string, Matrix> adjs = new Dictionary<string, Matrix>();
                Matrix rotforw = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitX, -(float)(Direction.Pitch / 2f * Utilities.PI180)));
                adjs["spine04"] = rotforw;
                SingleAnimationNode hand = tAnim.GetNode("metacarpal2.l");
                Matrix m4 = Matrix.CreateScale(1.5f, 1.5f, 1.5f)
                    * (Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)((-Direction.Yaw + 270) * Utilities.PI180) % 360f))
                    * hand.GetBoneTotalMatrix(aTTime, adjs));
                OpenTK.Matrix4 bonemat = new OpenTK.Matrix4(m4.M11, m4.M12, m4.M13, m4.M14, m4.M21, m4.M22, m4.M23, m4.M24,
                    m4.M31, m4.M32, m4.M33, m4.M34, m4.M41, m4.M42, m4.M43, m4.M44);
                GL.UniformMatrix4(10, false, ref bonemat);
                mod.LoadSkin(TheClient.Textures);
                mod.Draw();
                bonemat = OpenTK.Matrix4.Identity;
                GL.UniformMatrix4(10, false, ref bonemat);
                if (!TheClient.RenderingShadows)
                {
                    TheClient.Rendering.SetReflectionAmt(0f);
                }
            }
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

        public bool Walk;

        public bool Sprint;

        public bool Forward;

        public bool Backward;

        public bool Leftward;

        public bool Rightward;

        public bool Downward;

        public Location Position;

        public Location Velocity;
    }
}
