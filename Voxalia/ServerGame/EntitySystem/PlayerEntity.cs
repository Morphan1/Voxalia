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

        public int ViewRadiusInChunks = 8;

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

        public ConvexShape WheelShape = null;

        public PlayerEntity(WorldSystem.Region tregion, Connection conn)
            : base(tregion, true, 100f)
        {
            Network = conn;
            SetMass(tmass / 2f);
            Shape = new BoxShape((float)HalfSize.X * 2f, (float)HalfSize.Y * 2f, (float)(HalfSize.Z * 2f));
            WheelShape = new SphereShape((float)HalfSize.X);
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            Items = new PlayerInventory(this);
            Items.GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White.ToArgb(), "items/common/hand.dae", true));
            Items.GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White.ToArgb(), "items/common/fist.dae", true));
            Items.GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White.ToArgb(), "items/common/hook.dae", true));
            Items.GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            Items.GiveItem(new ItemStack("flashantilight", TheServer, 1, "items/common/flashlight_ico", "Flashantilight", "Lights things down!", Color.White.ToArgb(), "items/common/flashlight.dae", false));
            Items.GiveItem(new ItemStack("sun_angler", TheServer, 1, "items/tools/sun_angler", "Sun Angler", "Moves the sun itself!", Color.White.ToArgb(), "items/tools/sun_angler.dae", false));
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/grass_side", "Grass", "Grassy!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.GRASS });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/db_top", "DebugBlock", "Not buggy!", Color.White.ToArgb(), "items/block.dae", false) { Datum = (int)Material.DEBUG });
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

        public BEPUphysics.Entities.Entity WheelBody;

        public BEPUphysics.Constraints.TwoEntity.Joints.BallSocketJoint bsj;

        public override void SpawnBody()
        {
            base.SpawnBody();
            Body.LinearDamping = 0;
            Body.AngularDamping = 1;
            WheelBody = new BEPUphysics.Entities.Entity(WheelShape, tmass / 2f);
            WheelBody.Orientation = Quaternion.Identity;
            WheelBody.CollisionInformation.CollisionRules.Group = CollisionUtil.Solid;
            WheelBody.Position = Body.Position + new Vector3(0, 0, -(float)HalfSize.Z);
            WheelBody.CollisionInformation.CollisionRules.Specific.Add(Body.CollisionInformation.CollisionRules, BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase);
            Body.CollisionInformation.CollisionRules.Specific.Add(WheelBody.CollisionInformation.CollisionRules, BEPUphysics.CollisionRuleManagement.CollisionRule.NoBroadPhase);
            WheelBody.Tag = this;
            WheelBody.AngularDamping = 0.9f;
            WheelBody.LinearDamping = 0.9f;
            TheRegion.PhysicsWorld.Add(WheelBody);
            WheelBody.Gravity = Body.Gravity;
            bsj = new BEPUphysics.Constraints.TwoEntity.Joints.BallSocketJoint(Body, WheelBody, WheelBody.Position);
            TheRegion.PhysicsWorld.Add(bsj);
            if (CursorMarker == null)
            {
                CursorMarker = new CubeEntity(new Location(0.01, 0.01, 0.01), TheRegion, 0);
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                TheRegion.SpawnEntity(CursorMarker);
            }
        }

        public override void DestroyBody()
        {
            if (bsj != null)
            {
                TheRegion.PhysicsWorld.Remove(bsj);
                bsj = null;
            }
            base.DestroyBody();
            if (WheelBody != null)
            {
                TheRegion.PhysicsWorld.Remove(WheelBody);
                WheelBody = null;
            }
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
            bool fly = false;
            CollisionResult crGround = TheRegion.Collision.CuboidLineTrace(new Location(HalfSize.X - 0.01f, HalfSize.Y - 0.01f, 0.1f), GetPosition(), GetPosition() - new Location(0, 0, 0.1f), IgnoreThis);
            if (Upward && !fly && !pup && crGround.Hit && GetVelocity().Z < 1f)
            {
                Vector3 imp = (Location.UnitZ * GetMass() * 8f).ToBVector();
                Body.LinearMomentum = Vector3.Zero;
                Body.LinearVelocity = new Vector3(WheelBody.LinearVelocity.X, WheelBody.LinearVelocity.Y, 0);
                WheelBody.LinearMomentum = Vector3.Zero;
                WheelBody.LinearVelocity = new Vector3(WheelBody.LinearVelocity.X, WheelBody.LinearVelocity.Y, 0);
                Body.ApplyLinearImpulse(ref imp);
                WheelBody.LinearVelocity = Body.LinearVelocity;
                Body.ActivityInformation.Activate();
                imp = -imp;
                if (crGround.HitEnt != null)
                {
                    crGround.HitEnt.ApplyLinearImpulse(ref imp);
                    crGround.HitEnt.ActivityInformation.Activate();
                }
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            Location movement = new Location(0, 0, 0);
            if (Leftward)
            {
                movement.Y = -1;
            }
            if (Rightward)
            {
                movement.Y = 1;
            }
            if (Backward)
            {
                movement.X = 1;
            }
            if (Forward)
            {
                movement.X = -1;
            }
            if (movement.LengthSquared() > 0)
            {
                movement = Utilities.RotateVector(movement, Direction.Yaw * Utilities.PI180, fly ? Direction.Pitch * Utilities.PI180 : 0).Normalize();
            }
            Location intent_vel = movement * MoveSpeed * (Walk ? 0.7f : 1f) * GetMass() / 50;
            if (Stance == PlayerStance.CROUCH)
            {
                intent_vel *= 0.5f;
            }
            else if (Stance == PlayerStance.CRAWL)
            {
                intent_vel *= 0.3f;
            }
            if (ItemDoSpeedMod)
            {
                intent_vel *= ItemSpeedMod;
            }
            Location pvel = intent_vel - (fly ? Location.Zero : GetVelocity());
            if (!fly)
            {
                pvel.Z = 0;
            }
            if (pvel.LengthSquared() > MoveRateCap * MoveRateCap)
            {
                pvel = pvel.Normalize() * MoveRateCap;
            }
            pvel *= TheRegion.Delta * (crGround.Hit ? 1f : 0.1f);
            if (!fly)
            {
                Vector3 move = new Vector3((float)pvel.X, (float)pvel.Y, 0);
                Body.ApplyLinearImpulse(ref move);
                Body.ActivityInformation.Activate();
                WheelBody.ApplyLinearImpulse(ref move);
                WheelBody.ActivityInformation.Activate();
            }
            else
            {
                SetPosition(GetPosition() + pvel / 200);
            }
            CursorMarker.SetPosition(GetEyePosition() + ForwardVector() * 0.9f);
            CursorMarker.SetOrientation(Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), (float)(Direction.Pitch * Utilities.PI180)) * // TODO: ensure pitch works properly
                Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), (float)(Direction.Yaw * Utilities.PI180)));
            base.SetOrientation(Quaternion.Identity);
            PlayerUpdatePacketOut pupo = new PlayerUpdatePacketOut(this);
            for (int i = 0; i < TheServer.Players.Count; i++)
            {
                if (TheServer.Players[i] != this)
                {
                    TheServer.Players[i].Network.SendPacket(pupo);
                }
            }
            if (GetVelocity().LengthSquared() > 1)
            {
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 1);
                SetAnimation("human/" + StanceName() + "/walk_lowquality", 2);
            }
            else
            {
                SetAnimation("human/" + StanceName() + "/idle01", 1);
                SetAnimation("human/" + StanceName() + "/idle01", 2);
            }
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
                    TrySet(pos, ViewRadiusInChunks / 4, 0, 1);
                    TrySet(pos, ViewRadiusInChunks / 2, 0, 1);
                    //TrySet(pos, ViewRadiusInChunks, 0, 5);
                    loadedInitially = true;
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 1));
                }
                else
                {
                    // TODO: Better system -> async?
                    TrySet(pos, 1, 5, 1);
                    TrySet(pos, ViewRadiusInChunks / 4, 20, 1); // TODO: Closer chunks -> send sooner?
                    TrySet(pos, ViewRadiusInChunks / 2, 20, 1);
                    //TrySet(pos, ViewRadiusInChunks, 20, 5);
                    TheServer.Schedule.ScheduleSyncTask(() =>
                    {
                        ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                    }, 21);
                }
                pChunkLoc = cpos;
            }
            base.Tick();
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
            return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8);
            /*
            if (tAnim != null)
            {
                SingleAnimationNode head = tAnim.GetNode("head");
                Matrix m4 = head.GetBoneTotalMatrix(0);
                m4.Transpose();
                return GetPosition() + Location.FromBVector(m4.Translation);
            }
            else
            {
                return GetPosition() + new Location(0, 0, HalfSize.Z * 1.8f);
            }*/
        }

        public override Location GetPosition()
        {
            return base.GetPosition() - new Location(0, 0, HalfSize.Z + HalfSize.X);
        }

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos + new Location(0, 0, HalfSize.Z + HalfSize.X));
            if (WheelBody != null)
            {
                WheelBody.Position = pos.ToBVector() + new Vector3(0, 0, (float)HalfSize.X);
            }
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

        public PlayerStance Stance = PlayerStance.STAND;

        public string StanceName()
        {
            return Stance.ToString().ToLower();
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
