using System.Collections.Generic;
using System.Drawing;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem;
using BEPUutilities;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlayerEntity: EntityLiving
    {
        public Location HalfSize = new Location(0.55f, 0.55f, 1.3f);

        public Connection Network;

        public Connection ChunkNetwork;

        public string Name;

        public string Host;

        public string Port;

        public string IP;

        public byte LastPingByte = 0;

        public byte LastCPingByte = 0;

        public bool Upward = false;
        public bool Forward = false;
        public bool Backward = false;
        public bool Leftward = false;
        public bool Rightward = false;
        public bool Walk = false;

        public bool Click = false;
        public bool AltClick = false;

        bool pkick = false;

        public bool FlashLightOn = false;

        public PlayerInventory Items;

        public SingleAnimation hAnim = null;
        public SingleAnimation tAnim = null;
        public SingleAnimation lAnim = null;

        public int ViewRadiusInChunks = 4;

        public void Kick(string message)
        {
            if (pkick)
            {
                return;
            }
            pkick = true;
            if (Network.Alive)
            {
                Network.SendMessage("Kicking you: " + message);
                Network.Alive = false;
                Network.PrimarySocket.Close(5);
            }
            // TODO: Broadcast kick message
            SysConsole.Output(OutputType.INFO, "Kicking " + this.ToString() + ": " + message);
            if (IsSpawned)
            {
                ItemStack it = Items.GetItemForSlot(Items.cItem);
                it.Info.SwitchFrom(this, it);
                HookItem.RemoveHook(this);
                TheRegion.DespawnEntity(this);
            }
        }

        public float tmass = 100;

        public Location Direction;

        public CubeEntity CursorMarker = null;

        bool pup = false;

        public JointBallSocket GrabJoint = null;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public double ItemStartClickTime = -1;

        public bool WaitingForClickRelease = false;
        
        public PlayerEntity(WorldSystem.Region tregion, Connection conn)
            : base(tregion, true, 100f)
        {
            Network = conn;
            SetMass(tmass / 2f);
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            Items = new PlayerInventory(this);
            Items.GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White.ToArgb(), "items/common/hand.dae", true));
            Items.GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White.ToArgb(), "items/common/fist.dae", true));
            Items.GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White.ToArgb(), "items/common/hook.dae", true));
            Items.GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            Items.GiveItem(new ItemStack("flashantilight", TheServer, 1, "items/common/flashlight_ico", "Flashantilight", "Lights things down!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            Items.GiveItem(new ItemStack("sun_angler", TheServer, 1, "items/tools/sun_angler", "Sun Angler", "Moves the sun itself!", Color.White.ToArgb(), "items/tools/sun_angler.dae", false));
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/dirt", "Dirt", "Dirty!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.DIRT });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/stone", "Stone", "Gets things stoned!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.STONE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/grass_side", "Grass", "Grassy!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.GRASS });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/db_top", "DebugBlock", "Not buggy!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.DEBUG });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/concrete", "Concrete", "Solid!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.CONCRETE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/liquid/water", "Water", "Wet!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.WATER });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/transparent/leaves_basic1", "Leaves", "Transparent in parts!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.LEAVES1 });
            Items.GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/weapons/9mm_pistol_ico", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb(), "items/weapons/silenced_pistol.dae", false));
            Items.GiveItem(new ItemStack("shotgun_gun", TheServer, 1, "items/weapons/shotgun_ico", "Shotgun", "It shoots many bullets!", Color.White.ToArgb(), "items/weapons/shotgun.dae", false));
            Items.GiveItem(new ItemStack("bow", TheServer, 1, "items/weapons/bow_ico", "Bow", "It shoots arrows!", Color.White.ToArgb(), "items/weapons/bow.dae", false));
            Items.GiveItem(new ItemStack("explodobow", TheServer, 1, "items/weapons/bow_ico", "ExplodoBow", "It shoots arrows that go boom!", Color.White.ToArgb(), "items/weapons/bow.dae", false));
            Items.GiveItem(new ItemStack("rifle_gun", TheServer, 1, "items/weapons/rifle_ico", "Assault Rifle", "It shoots rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/m4a1.dae", false));
            Items.GiveItem(new ItemStack("rifle_gun", TheServer, 1, "items/weapons/minigun_ico", "Minigun", "It shoots ^ivery^r rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/minigun.dae", false,
                "firerate_mod", "0.1", "spread_mod", "5", "clipsize_mod", "10", "shots_mod", "3"));
            Items.GiveItem(new ItemStack("bullet", "9mm_ammo", TheServer, 100, "items/weapons/ammo/9mm_round_ico", "9mm Ammo", "Nine whole millimeters!", Color.White.ToArgb(), "items/weapons/ammo/9mm_round.dae", false));
            Items.GiveItem(new ItemStack("bullet", "shotgun_ammo", TheServer, 100, "items/weapons/ammo/shotgun_shell_ico", "Shotgun Ammo", "Always travels in packs!", Color.White.ToArgb(), "items/weapons/ammo/shotgun_shell.dae", false));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", Color.White.ToArgb(), "items/weapons/ammo/rifle_round.dae", false));
            SetHealth(Health);
            CGroup = CollisionUtil.Player;
        }

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
            CBody.StepManager.MaximumStepHeight = 0.05f;
            CBody.StepManager.MinimumDownStepHeight = 0.05f;
            TheRegion.PhysicsWorld.Add(CBody);
            if (CursorMarker == null)
            {
                CursorMarker = new CubeEntity(new Location(0.01, 0.01, 0.01), TheRegion, 0);
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                TheRegion.SpawnEntity(CursorMarker);
            }
        }

        public float CBRadius = 0.01f;

        public float CBMaxTractionSlope = 1.0f;

        public float CBMaxSupportSlope = 1.3f;

        public float CBStandSpeed = 5.0f;

        public float CBCrouchSpeed = 2.5f;

        public float CBSlideSpeed = 2f;

        public float CBAirSpeed = 0.5f;

        public float CBTractionForce = 100f;

        public float CBSlideForce = 50f;

        public float CBAirForce = 25f;

        public float CBJumpSpeed = 10f;

        public float CBSlideJumpSpeed = 5f;

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
            if (CursorMarker.IsSpawned)
            {
                TheRegion.DespawnEntity(CursorMarker);
                CursorMarker = null;
            }
        }

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                if (hAnim != null && hAnim.Name == anim)
                {
                    return;
                }
                hAnim = TheServer.Animations.GetAnimation(anim);
            }
            else if (mode == 1)
            {
                if (tAnim != null && tAnim.Name == anim)
                {
                    return;
                }
                tAnim = TheServer.Animations.GetAnimation(anim);
            }
            else
            {
                if (lAnim != null && lAnim.Name == anim)
                {
                    return;
                }
                lAnim = TheServer.Animations.GetAnimation(anim);
            }
            TheRegion.SendToAll(new AnimationPacketOut(this, anim, mode));
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

        public float ItemSpeedMod = 1;
        public bool ItemDoSpeedMod = false;

        public override void Tick()
        {
            if (!IsSpawned)
            {
                return;
            }
            if (TheRegion.Delta <= 0)
            {
                return;
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
            bool fly = false;
            if (Upward && !fly && !pup && CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            CBody.StandingSpeed = CBStandSpeed;
            CBody.CrouchingSpeed = CBCrouchSpeed;
            if (ItemDoSpeedMod)
            {
                CBody.StandingSpeed = CBStandSpeed * ItemSpeedMod;
                CBody.CrouchingSpeed = CBCrouchSpeed * ItemSpeedMod;
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
            CursorMarker.SetPosition(GetEyePosition() + ForwardVector() * 0.9f);
            CursorMarker.SetOrientation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(Direction.Pitch * Utilities.PI180)) * // TODO: ensure pitch works properly
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(Direction.Yaw * Utilities.PI180)));
            //base.SetOrientation(Quaternion.Identity);
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
            }
            // TODO: Animation!
            /*
            if (GetVelocity().LengthSquared() > 1)
            {
                // TODO: Replicate animation automation on client?
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 1);
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 2);
            }
            else
            {
                SetAnimation("human/" + StanceName() + "/idle01", 1);
                SetAnimation("human/" + StanceName() + "/idle01", 2);
            }
            */
            ItemStack cit = Items.GetItemForSlot(Items.cItem);
            if (Click)
            {
                cit.Info.Click(this, cit);
                LastClick = TheRegion.GlobalTickTime;
                WasClicking = true;
            }
            else if (WasClicking)
            {
                cit.Info.ReleaseClick(this, cit);
                WasClicking = false;
            }
            if (AltClick)
            {
                cit.Info.AltClick(this, cit);
                LastAltClick = TheRegion.GlobalTickTime;
                WasAltClicking = true;
            }
            else if (WasAltClicking)
            {
                cit.Info.ReleaseAltClick(this, cit);
                WasAltClicking = false;
            }
            cit.Info.Tick(this, cit);
            Location pos = GetPosition();
            Location cpos = TheRegion.ChunkLocFor(pos);
            if (cpos != pChunkLoc)
            {
                // TODO: Move to a separate method that's called once on startup + at every teleport... also, asyncify
                if (!loadedInitially)
                {
                    TrySet(pos, 1, 0, 1);
                    TrySet(pos, ViewRadiusInChunks / 2, 0, 1);
                    TrySet(pos, ViewRadiusInChunks, 0, 1);
                    loadedInitially = true;
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 1));
                }
                else
                {
                    // TODO: Better system -> async?
                    TrySet(pos, 1, 5, 1);
                    TrySet(pos, ViewRadiusInChunks / 2, 20, 1); // TODO: Closer chunks -> send sooner?
                    TrySet(pos, ViewRadiusInChunks, 20, 1);
                    TheServer.Schedule.ScheduleSyncTask(() =>
                    {
                        ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                    }, 21);
                }
                pChunkLoc = cpos;
            }
            //base.Tick();
        }

        Location pChunkLoc = new Location(0.5);
        
        bool loadedInitially = false;

        public void TrySet(Location pos, int VIEWRAD, float atime, int posMult)
        {
            for (int x = -VIEWRAD; x <= VIEWRAD; x++)
            {
                for (int y = -VIEWRAD; y <= VIEWRAD; y++)
                {
                    for (int z = -VIEWRAD; z <= VIEWRAD; z++)
                    {
                        TryChunk(pos + new Location(30 * x, 30 * y, 30 * z), atime, posMult);
                    }
                }
            }
        }

        public void TryChunk(Location worldPos, float atime, int posMult) // TODO: Efficiency?
        {
            worldPos = TheRegion.ChunkLocFor(worldPos);
            ChunkAwarenessInfo cai = new ChunkAwarenessInfo() { ChunkPos = worldPos, LOD = posMult };
            if (!ChunksAwareOf.ContainsKey(worldPos) || ChunksAwareOf[worldPos].LOD > posMult) // TODO: Efficiency - TryGetValue?
            { // TODO: Or ATime > awareOf.remTime?
                if (ChunksAwareOf.ContainsKey(worldPos)) // TODO: Efficiency - TryGetValue?
                {
                    ChunkAwarenessInfo acai = ChunksAwareOf[worldPos];
                    if (acai != null && acai.SendToClient != null && acai.SendToClient.Time > 0)
                    {
                        TheServer.Schedule.DescheduleSyncTask(acai.SendToClient);
                    }
                }
                if (atime == 0)
                {
                    Chunk chk = TheRegion.LoadChunk(worldPos);
                    ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk, posMult));
                    ChunksAwareOf.Remove(worldPos);
                    ChunksAwareOf.Add(worldPos, new ChunkAwarenessInfo() { ChunkPos = worldPos, LOD = posMult, SendToClient = null });
                }
                else
                {
                    
                    SyncScheduleItem item = TheServer.Schedule.ScheduleSyncTask(() => { if (!pkick) { Chunk chk = TheRegion.LoadChunk(worldPos); ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk, posMult)); } }, Utilities.UtilRandom.NextDouble() * atime);
                    ChunksAwareOf.Remove(worldPos);
                    ChunksAwareOf.Add(worldPos, new ChunkAwarenessInfo() { ChunkPos = worldPos, LOD = posMult, SendToClient = item });
                }
                // TODO: Add a note of whether the client has acknowledged the chunk's reception... (Also, chunk reception ack packet) so block edit notes can be delayed.
            }
        }

        public Dictionary<Location, ChunkAwarenessInfo> ChunksAwareOf = new Dictionary<Location, ChunkAwarenessInfo>();

        public double LastClick = 0;

        public double LastGunShot = 0;

        public double LastBlockBreak = 0;

        public double LastBlockPlace = 0;

        public bool WasClicking = false;

        public double LastAltClick = 0;

        public bool WasAltClicking = false;

        public float MoveSpeed = 960;
        public float MoveRateCap = 1920;

        public Location ForwardVector()
        {
            return Utilities.ForwardVector_Deg(Direction.Yaw, Direction.Pitch);
        }

        public Location GetEyePosition()
        {
            return GetPosition() + new Location(0, 0, HalfSize.Z * (CBody.StanceManager.CurrentStance == Stance.Standing ? 1.8 : 1.5));
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

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z));
        }

        public void Teleport(Location pos)
        {
            SetPosition(pos);
            Network.SendPacket(new TeleportPacketOut(pos));
        }

        public Location GetCenter()
        {
            return base.GetPosition();
        }

        public override string ToString()
        {
            return Name;
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

        public YourStatusFlags Flags = YourStatusFlags.NONE;

        public override void SetHealth(float health)
        {
            base.SetHealth(health);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth(), Flags));
        }

        public override void SetMaxHealth(float maxhealth)
        {
            base.SetMaxHealth(maxhealth);
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth(), Flags));
        }

        public override void Die()
        {
            SetHealth(MaxHealth);
            if (TheRegion.SpawnPoints.Count == 0)
            {
                SysConsole.Output(OutputType.WARNING, "No spawn points... generating one!");
                TheRegion.SpawnEntity(new SpawnPointEntity(TheRegion) { Position = new Location(0, 0, 25) });
            }
            SpawnPointEntity spe = null;
            for (int i = 0; i < 10; i++)
            {
                spe = TheRegion.SpawnPoints[Utilities.UtilRandom.Next(TheRegion.SpawnPoints.Count)];
                if (!TheRegion.Collision.CuboidLineTrace(HalfSize, spe.GetPosition(), spe.GetPosition() + new Location(0, 0, 0.01f)).Hit)
                {
                    break;
                }
            }
            SetPosition(spe.GetPosition());
        }
    }

    public class ChunkAwarenessInfo
    {
        public Location ChunkPos;

        public int LOD;

        public SyncScheduleItem SendToClient;

        public override int GetHashCode()
        {
            return ChunkPos.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            return ChunkPos.Equals(((ChunkAwarenessInfo)obj).ChunkPos);
        }

        public static bool operator ==(ChunkAwarenessInfo cai, ChunkAwarenessInfo cai2)
        {
            return cai.Equals(cai2);
        }

        public static bool operator !=(ChunkAwarenessInfo cai, ChunkAwarenessInfo cai2)
        {
            return !cai.Equals(cai2);
        }
    }
}
