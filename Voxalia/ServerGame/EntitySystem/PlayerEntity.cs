using System;
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
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlayerEntity: EntityLiving
    {
        /// <summary>
        /// Half the size of the player, if needed for a cuboid trace.
        /// </summary>
        public Location HalfSize = new Location(0.55f, 0.55f, 1.3f);

        /// <summary>
        /// The primary connection to the player over the network.
        /// </summary>
        public Connection Network;

        /// <summary>
        /// The secondary (chunk packets) connection to the player over the network.
        /// </summary>
        public Connection ChunkNetwork;

        /// <summary>
        /// The global time of the last received KeysPacketIn.
        /// </summary>
        public double LastKPI = 0;

        /// <summary>
        /// The name of the player.
        /// </summary>
        public string Name;

        /// <summary>
        /// The address the player connected to, to join this server.
        /// </summary>
        public string Host;

        /// <summary>
        /// The port the player connected to, to join this server.
        /// </summary>
        public string Port;

        /// <summary>
        /// The IP address of this player.
        /// </summary>
        public string IP;

        public byte LastPingByte = 0;

        public byte LastCPingByte = 0;

        public bool Upward = false;

        public bool Forward = false;

        public bool Backward = false;

        public bool Leftward = false;

        public bool Rightward = false;

        public bool Walk = false;

        public bool Sprint = false;
        
        public bool Downward = false;

        /// <summary>
        /// Returns whether this player is currently crouching.
        /// </summary>
        public bool IsCrouching
        {
            get
            {
                return Downward || DesiredStance == Stance.Crouching;
            }
        }

        public bool Click = false;

        public bool AltClick = false;

        bool pkick = false;

        public bool FlashLightOn = false;

        /// <summary>
        /// The player's primary inventory.
        /// TODO: Split to 'quickbar' and 'maininventory'!
        /// </summary>
        public PlayerInventory Items;

        /// <summary>
        /// The animation of the player's head.
        /// </summary>
        public SingleAnimation hAnim = null;

        /// <summary>
        /// The animation of the player's torso.
        /// </summary>
        public SingleAnimation tAnim = null;

        /// <summary>
        /// The animation of the player's legs.
        /// </summary>
        public SingleAnimation lAnim = null;

        /// <summary>
        /// How far (in chunks) the player can see, as a cubic radius, excluding the chunk the player is in.
        /// </summary>
        public int ViewRadiusInChunks = 4;

        /// <summary>
        /// Kicks the player from the server with a specified message.
        /// </summary>
        /// <param name="message">The kick reason.</param>
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

        /// <summary>
        /// The default mass of the player.
        /// </summary>
        public float tmass = 100;

        /// <summary>
        /// The direction the player is currently facing, as Yaw/Pitch.
        /// </summary>
        public Location Direction;

        public ModelEntity CursorMarker = null;

        public bool pup = false;

        public JointBallSocket GrabJoint = null;

        public List<HookInfo> Hooks = new List<HookInfo>();

        public double ItemCooldown = 0;

        public double ItemStartClickTime = -1;

        public bool WaitingForClickRelease = false;

        // TODO: Dictionary<breadcrumb id: int, List<Location>> ?
        public List<Location> Breadcrumbs = new List<Location>();
        
        public PlayerEntity(WorldSystem.Region tregion, Connection conn)
            : base(tregion, true, 100f)
        {
            Network = conn;
            SetMass(tmass);
            CanRotate = false;
            SetPosition(new Location(0, 0, 50));
            Items = new PlayerInventory(this);
            Items.GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White.ToArgb(), "items/common/hand", true));
            Items.GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White.ToArgb(), "items/common/fist", true));
            Items.GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White.ToArgb(), "items/common/hook", true));
            Items.GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White.ToArgb(), "items/common/flashlight", false));
            Items.GiveItem(new ItemStack("flashantilight", TheServer, 1, "items/common/flashlight_ico", "Flashantilight", "Lights things down!", Color.White.ToArgb(), "items/common/flashlight", false));
            Items.GiveItem(new ItemStack("sun_angler", TheServer, 1, "items/tools/sun_angler", "Sun Angler", "Moves the sun itself!", Color.White.ToArgb(), "items/tools/sun_angler", false));
            Items.GiveItem(new ItemStack("breadcrumb", TheServer, 1, "items/common/breadcrumbs", "Bread Crumbs", "Finds the way back, even over the river and through the woods!", Color.White.ToArgb(), "items/common/breadcrumbs", false));
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/dirt", "Dirt", "Dirty!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.DIRT });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/stone", "Stone", "Gets things stoned!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.STONE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/grass_forest_side", "Forest Grass", "Grassy!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.GRASS_FOREST });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/grass_plains_side", "Plains Grass", "Light and grassy!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.GRASS_PLAINS });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/special/db_top", "DebugBlock", "Not buggy!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.DEBUG });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/made/concrete", "Concrete", "Solid!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.CONCRETE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/metals/steel_solid", "Solid Steel", "*very* solid!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.STEEL_SOLID });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/metals/steel_plate", "Plated Steel", "Plated for extra strength!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.STEEL_PLATE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/wood", "Wood Log", "Makes a rather woody sound, doe'n'it?!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.LOG_OAK });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/liquid/water", "Water", "Wet!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.WATER });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/liquid/smoke", "Smoke", "Gaseous!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.SMOKE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/liquid/slipgoo", "Slip Goo", "Slippery!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.SLIPGOO });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/snow", "Snow", "Cold!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.SNOW });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/natural/sand", "Sand", "Literally, it's sand!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.SAND });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/transparent/leaves_basic1", "Leaves", "Transparent in parts!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.LEAVES1 });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/ore/tin_ore", "Tin Ore", "Tinny!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.TIN_ORE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/ore/tin_ore_sparse", "Sparse Tin Ore", "Slightly tinny!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.TIN_ORE_SPARSE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/ore/copper_ore", "Copper Ore", "Coppery!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.COPPER_ORE });
            Items.GiveItem(new ItemStack("block", TheServer, 100, "blocks/solid/ore/copper_ore_sparse", "Sparse Copper Ore", "Slightly coppery!", Color.White.ToArgb(), "items/block", false) { Datum = (int)Material.COPPER_ORE_SPARSE });
            Items.GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/weapons/9mm_pistol_ico", "9mm Pistol", "It shoots bullets!", Color.White.ToArgb(), "items/weapons/silenced_pistol", false));
            Items.GiveItem(new ItemStack("shotgun_gun", TheServer, 1, "items/weapons/shotgun_ico", "Shotgun", "It shoots many bullets!", Color.White.ToArgb(), "items/weapons/shotgun", false));
            Items.GiveItem(new ItemStack("bow", TheServer, 1, "items/weapons/bow_ico", "Bow", "It shoots arrows!", Color.White.ToArgb(), "items/weapons/bow", false));
            Items.GiveItem(new ItemStack("explodobow", TheServer, 1, "items/weapons/bow_ico", "ExplodoBow", "It shoots arrows that go boom!", Color.White.ToArgb(), "items/weapons/bow", false));
            Items.GiveItem(new ItemStack("rifle_gun", TheServer, 1, "items/weapons/rifle_ico", "Assault Rifle", "It shoots rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/m4a1", false));
            Items.GiveItem(new ItemStack("rifle_gun", TheServer, 1, "items/weapons/minigun_ico", "Minigun", "It shoots ^bvery^B rapid-fire bullets!", Color.White.ToArgb(), "items/weapons/minigun", false,
                "firerate_mod", "0.1", "spread_mod", "5", "clipsize_mod", "10", "shots_mod", "3"));
            Items.GiveItem(new ItemStack("bullet", "9mm_ammo", TheServer, 100, "items/weapons/ammo/9mm_round_ico", "9mm Ammo", "Nine whole millimeters!", Color.White.ToArgb(), "items/weapons/ammo/9mm_round", false));
            Items.GiveItem(new ItemStack("bullet", "shotgun_ammo", TheServer, 100, "items/weapons/ammo/shotgun_shell_ico", "Shotgun Ammo", "Always travels in packs!", Color.White.ToArgb(), "items/weapons/ammo/shotgun_shell", false));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", Color.White.ToArgb(), "items/weapons/ammo/rifle_round", false));
            Items.GiveItem(new ItemStack("glowstick", TheServer, 10, "items/common/glowstick_ico", "Glowstick", "Pretty colors!", Color.Cyan.ToArgb(), "items/common/glowstick", false));
            Items.GiveItem(new ItemStack("smokegrenade", TheServer, 10, "items/weapons/smokegrenade_ico", "Smoke Grenade", "Not safe around those with asthma!", Color.FromArgb(255, 128, 128).ToArgb(), "items/weapons/smokegrenade", false));
            Items.GiveItem(new ItemStack("smokegrenade", TheServer, 10, "items/weapons/smokesignal_ico", "Smoke Signal", "Avoid when hiding from aircraft!", Color.FromArgb(255, 128, 128).ToArgb(), "items/weapons/smokesignal", false, "big_smoke", "1"));
            Items.GiveItem(new ItemStack("smokemachine", TheServer, 10, "items/common/smokemachine_ico", "Smoke Machine", "Do not inhale!", Color.White.ToArgb(), "items/common/smokemachine", false));
            SetHealth(Health);
            CGroup = CollisionUtil.Player;
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
            CBody = new CharacterController(WorldTransform.Translation, (float)HalfSize.Z * 2f, (float)HalfSize.Z * 1.1f,
                (float)HalfSize.Z * 1f, CBRadius, CBMargin, Mass, CBMaxTractionSlope, CBMaxSupportSlope, CBStandSpeed, CBCrouchSpeed, CBProneSpeed,
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
            if (CursorMarker == null)
            {
                CursorMarker =  new ModelEntity("cube", TheRegion);
                CursorMarker.scale = new Location(0.1f, 0.1f, 0.1f);
                CursorMarker.mode = ModelCollisionMode.AABB;
                CursorMarker.CGroup = CollisionUtil.NonSolid;
                CursorMarker.Visible = false;
                TheRegion.SpawnEntity(CursorMarker);
            }
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
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.FLYING, 1));
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.MASS, 0));
        }

        public void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            IsFlying = false;
            SetMass(PreFlyMass);
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.FLYING, 0));
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.MASS, PreFlyMass));
        }

        public Stance DesiredStance = Stance.Standing;

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
            if (Upward && !IsFlying && !pup && CBody.SupportFinder.HasSupport)
            {
                CBody.Jump();
                pup = true;
            }
            else if (!Upward)
            {
                pup = false;
            }
            float speedmod = Sprint ? 1.5f : (Walk ? 0.5f : 1f);
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
            Vector3 movement = new Vector3(0, 0, 0);
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
                SetPosition(GetPosition() + forw * TheRegion.Delta * CBStandSpeed * 2 * (Sprint ? 2: (Walk ? 0.5 : 1)));
                CBody.HorizontalMotionConstraint.MovementDirection = Vector2.Zero;
                Body.LinearVelocity = new Vector3(0, 0, 0);
            }
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
            if (GetVelocity().LengthSquared() > 1)
            {
                // TODO: Replicate animation automation on client?
                SetAnimation("human/stand/run", 1);
                SetAnimation("human/stand/run", 2);
                SetAnimation("human/stand/run", 3);
            }
            else
            {
                SetAnimation("human/stand/idle01", 1);
                SetAnimation("human/stand/idle01", 2);
                SetAnimation("human/stand/idle01", 3);
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
                // TODO: Move to a separate method that's called once on startup + at every teleport... also, asyncify!
                if (!loadedInitially)
                {
                    TrySet(pos, 1, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks / 2, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks + 1, 0, 1, true);
                    loadedInitially = true;
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 1));
                }
                else
                {
                    // TODO: Better system -> async?
                    TrySet(pos, 1, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks / 2, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks, 0, 1, false);
                    TrySet(pos, ViewRadiusInChunks + 1, 0, 1, true);
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                }
                List<Location> removes = new List<Location>();
                foreach (ChunkAwarenessInfo ch in ChunksAwareOf.Values)
                {
                    if (!ShouldSeeChunk(ch.ChunkPos))
                    {
                        removes.Add(ch.ChunkPos);
                    }
                }
                foreach (Location loc in removes)
                {
                    ForgetChunk(loc);
                }
                pChunkLoc = cpos;
            }
            if (Breadcrumbs.Count > 0)
            {
                double dist = (GetPosition() - Breadcrumbs[Breadcrumbs.Count - 1]).LengthSquared();
                if (dist > BreadcrumbRadius * BreadcrumbRadius)
                {
                    Location one = Breadcrumbs[Breadcrumbs.Count - 1];
                    Location two = GetPosition().GetBlockLocation() + new Location(0.5f, 0.5f, 0.5f);
                    Breadcrumbs.Add((two - one).Normalize() * BreadcrumbRadius + one);
                    // TODO: Effect?
                }
            }
            base.Tick();
        }

        public bool ShouldSeeChunk(Location cpos)
        {
            Location wpos = TheRegion.ChunkLocFor(GetPosition());
            if (Math.Abs(cpos.X - wpos.X) > ViewRadiusInChunks
                || Math.Abs(cpos.Y - wpos.Y) > ViewRadiusInChunks
                || Math.Abs(cpos.Z - wpos.Z) > ViewRadiusInChunks)
            {
                return false;
            }
            return true;
        }
        
        public int BreadcrumbRadius = 6;

        Location pChunkLoc = new Location(0.5);
        
        bool loadedInitially = false;

        public void TrySet(Location pos, int VIEWRAD, float atime, int posMult, bool bg)
        {
            for (int x = -VIEWRAD; x <= VIEWRAD; x++)
            {
                for (int y = -VIEWRAD; y <= VIEWRAD; y++)
                {
                    for (int z = -VIEWRAD; z <= VIEWRAD; z++)
                    {
                        if (bg)
                        {
                            Location chl = TheRegion.ChunkLocFor(pos + new Location(30 * x, 30 * y, 30 * z));
                            TheRegion.LoadChunk_Background(chl);
                        }
                        else
                        {
                            TryChunk(pos + new Location(30 * x, 30 * y, 30 * z), atime, posMult);
                        }
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
            }
        }

        public Dictionary<Location, ChunkAwarenessInfo> ChunksAwareOf = new Dictionary<Location, ChunkAwarenessInfo>();

        public bool CanSeeChunk(Location cpos)
        {
            return ChunksAwareOf.ContainsKey(cpos);
        }

        public bool ForgetChunk(Location cpos)
        {
            if (ChunksAwareOf.Remove(cpos))
            {
                ChunkNetwork.SendPacket(new ChunkForgetPacketOut(cpos));
                return true;
            }
            return false;
        }

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
                TheRegion.SpawnEntity(new SpawnPointEntity(TheRegion) { Position = new Location(0, 0, 70) });
            }
            SpawnPointEntity spe = null; // TODO: Scrap old spawn point code, handle more sanely!
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
