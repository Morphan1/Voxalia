using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
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
using Frenetic;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class PlayerEntity: PhysicsEntity, EntityAnimated
    {
        public YourStatusFlags ServerFlags = YourStatusFlags.NONE;

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

        public Location ServerLocation = new Location(0, 0, 0);

        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Upward = false;
        public bool Click = false;
        public bool AltClick = false;
        public bool Walk = false;

        public float tmass = 100;

        bool pup = false;

        public Model model;
        
        public PlayerEntity(Region tregion)
            : base(tregion, true, true)
        {
            SetMass(tmass / 2f);
            CanRotate = false;
            EID = -1;
            model = TheClient.Models.GetModel("players/human_male_004");
            model.LoadSkin(TheClient.Textures);
            CGroup = CollisionUtil.Player;
            NMTWOWorld.ForceUpdater.Gravity = TheRegion.PhysicsWorld.ForceUpdater.Gravity;
            SetPosition(new Location(0, 0, 1000));
        }

        public bool IgnoreThis(BroadPhaseEntry entry)
        {
            if (entry is EntityCollidable && ((EntityCollidable)entry).Entity.Tag == this)
            {
                return false;
            }
            return TheRegion.Collision.ShouldCollide(entry);
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

#if NEW_CHUNKS
        Dictionary<Location, FullChunkObject> NMTWOMeshes = new Dictionary<Location, FullChunkObject>();
#else
        Dictionary<Location, InstancedMesh> NMTWOMeshes = new Dictionary<Location, InstancedMesh>();
#endif

        double lGTT = 0;

        double _gtt;
        long _id = -1;
        Location _pos;
        Location _vel;

        public void PacketFromServer(double gtt, long ID, Location pos, Location vel)
        {
            _gtt = gtt;
            _id = ID;
            _pos = pos;
            _vel = vel;
        }

        HashSet<Location> Quiet = new HashSet<Location>();

        public void UpdateForPacketFromServer(double gtt, long ID, Location pos, Location vel)
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
#if NEW_CHUNKS
                            FullChunkObject temp;
                            if ((!NMTWOMeshes.TryGetValue(ch, out temp)) || (temp.Shape != chunk.FCO.Shape))
                            {
                                if (temp != null)
                                {
                                    NMTWOWorld.Remove(temp);
                                    NMTWOMeshes.Remove(ch);
                                }
                                if (chunk.MeshShape != null)
                                {
                                    FullChunkObject im = new FullChunkObject(chunk.FCO.Position, (FullChunkShape)chunk.FCO.Shape);
                                    NMTWOWorld.Add(im);
                                    NMTWOMeshes[ch] = im;
                                }
                            }
#else
                            InstancedMesh temp;
                            if ((!NMTWOMeshes.TryGetValue(ch, out temp)) || (temp.Shape != chunk.MeshShape))
                            {
                                if (temp != null)
                                {
                                    NMTWOWorld.Remove(temp);
                                    NMTWOMeshes.Remove(ch);
                                }
                                if (chunk.MeshShape != null)
                                {
                                    InstancedMesh im = new InstancedMesh(chunk.MeshShape);
                                    NMTWOWorld.Add(im);
                                    NMTWOMeshes[ch] = im;
                                }
                            }
#endif
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
                    lPT = uis.GlobalTimeLocal;
                    NMTWOWorld.Update((float)delta);
                    FlyForth(NMTWOCBody, delta); // TODO: Entirely disregard NWTWOWorld if flying?
                }
                AddUIS();
                SetPosition(NMTWOGetPosition());
                SetVelocity(new Location(NMTWOCBody.Body.LinearVelocity));
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
                Walk = Walk,
                Forward = Forward,
                Backward = Backward,
                Leftward = Leftward,
                Rightward = Rightward,
                Direction = Direction,
                Position = GetPosition(),
                Velocity = GetVelocity(),
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
                  | (Click ? KeysPacketData.CLICK : 0) | (AltClick ? KeysPacketData.ALTCLICK : 0);
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
        }

        public void FlyForth(CharacterController cc, double delta)
        {
            if (IsFlying)
            {
                Location forw = Utilities.RotateVector(new Location(-cc.HorizontalMotionConstraint.MovementDirection.Y,
                    cc.HorizontalMotionConstraint.MovementDirection.X, 0), Direction.Yaw * Utilities.PI180, Direction.Pitch * Utilities.PI180);
                cc.Body.Position += (forw * delta * CBStandSpeed * 2 * (Upward ? 2 : 1)).ToBVector(); // TODO: Make upward go up?
                CBody.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
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

        public void SetMoveSpeed(CharacterController cc)
        {
            float speedmod = 1f;
            if (Click)
            {
                ItemStack item = TheClient.GetItemForSlot(TheClient.QuickBarPos);
                if (item.SharedAttributes.ContainsKey("charge") && item.SharedAttributes["charge"] == 1f)
                {
                    speedmod *= item.SharedAttributes["cspeedm"];
                }
            }
            Material mat = TheRegion.GetBlockMaterial(new Location(cc.Body.Position) + new Location(0, 0, -0.05f));
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
            float pitch = (float)(Utilities.UtilRandom.NextDouble() * 0.1 + 1.0 - 0.05);
            float volume = (float)Math.Min(((Utilities.UtilRandom.NextDouble() * 0.1 + 1.0 - 0.1) * 0.14 * GetVelocity().Length()), 1.0);
            // TODO: registry of some form?
            switch (sound)
            {
                case MaterialSound.GRASS:
                case MaterialSound.SAND:
                case MaterialSound.LEAVES:
                case MaterialSound.WOOD:
                case MaterialSound.METAL:
                case MaterialSound.DIRT:
                    // TODO: Don't manually search the sound list every time!
                    TheClient.Sounds.Play(TheClient.Sounds.GetSound("sfx/steps/humanoid/" + sound.ToString().ToLower() + (Utilities.UtilRandom.Next(4) + 1)), false, GetPosition(), pitch, volume);
                    break;
                default:
                    return;
            }
            SoundTimeout = (Utilities.UtilRandom.NextDouble() * 0.2 + 1.0) / GetVelocity().Length();
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
            SetMoveSpeed(CBody);
            UpdateLocalMovement();
            FlyForth(CBody, TheRegion.Delta);
            SetBodyMovement(CBody, lUIS);
            PlayRelevantSounds();
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
                UpdateForPacketFromServer(_gtt, _id, _pos, _vel);
                _id = -1;
            }
        }

        public CharacterController CBody;

        CharacterController GenCharCon()
        {
            // TODO: Better variable control! (Server should command every detail!)
            CharacterController cb = new CharacterController(ServerLocation.ToBVector(), (float)HalfSize.Z * 2f, (float)HalfSize.Z * 1.1f,
                (float)HalfSize.Z * 1f, CBRadius, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
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

        public CharacterController NMTWOCBody = null;
        
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
            NMTWOCBody = GenCharCon();
            NMTWOWorld.Add(NMTWOCBody);
        }

        public float CBProneSpeed = 1f;

        public float CBMargin = 0.01f;

        public float CBStepHeight = 0.6f;

        public float CBDownStepHeight = 0.6f;

        public float CBRadius = 0.55f;

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
            NMTWOWorld.Remove(NMTWOCBody);
        }

        public SpotLight Flashlight = null;

        public float MoveSpeed = 960;
        public float MoveRateCap = 1920;

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8: 1.5));
            /*
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("head");
                Matrix m4 = head.GetBoneTotalMatrix(0);
                m4.Transpose();
                return GetPosition() + new Location(m4.Translation);
            }
            else
            {
                return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8f);
            }*/
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
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
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
                base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
            }
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

        public override void SetVelocity(Location vel)
        {
            base.SetVelocity(vel);
        }

        public float Health;

        public float MaxHealth;

        public static OpenTK.Matrix4 PlayerAngleMat = OpenTK.Matrix4.CreateRotationZ((float)(270 * Utilities.PI180));

        public double aHTime;
        public double aTTime;
        public double aLTime;

        public override void Render()
        {
            if (TheClient.CVars.n_debugmovement.ValueB)
            {
                TheClient.Rendering.RenderLine(ServerLocation, GetPosition());
                TheClient.Rendering.RenderLineBox(ServerLocation + new Location(-0.2), ServerLocation + new Location(0.2));
            }
            if (TheClient.RenderingShadows || !TheClient.CVars.g_firstperson.ValueB)
            {
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
                model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
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

        public bool Forward;

        public bool Backward;

        public bool Leftward;

        public bool Rightward;

        public Location Position;

        public Location Velocity;
    }
}
