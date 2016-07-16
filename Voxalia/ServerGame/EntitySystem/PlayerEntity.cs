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
using Voxalia.ServerGame.OtherSystems;
using Voxalia.Shared.Collision;
using BEPUphysics.Character;
using FreneticScript;
using FreneticScript.TagHandlers.Objects;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlayerEntity: HumanoidEntity
    {
        public string SessionKey = null;

        public GameMode Mode = GameMode.SURVIVOR;

        public bool IsFirstJoin = true;

        public long[] UsagesTotal = new long[(int)NetUsageType.COUNT];

        public double SpawnedTime = 0;

        public bool SecureMovement = true;

        public void LoadFromYAML(YAMLConfiguration config)
        {
            string region = config.ReadString("region", null);
            if (region != null) // TODO: && TheServer.IsLoadedRegion(region)
            {
                // TODO: Set region!
            }
            if (!Enum.TryParse(config.ReadString("gamemode", "SURVIVOR"), out Mode))
            {
                SysConsole.Output(OutputType.WARNING, "Invalid gamemode for " + Name + ", reverting to SURVIVOR!");
                Mode = GameMode.SURVIVOR;
            }
            SetMaxHealth(config.ReadFloat("maxhealth", 100));
            SetHealth(config.ReadFloat("health", 100));
            if (config.ReadString("flying", "false").ToLowerFast() == "true") // TODO: ReadBoolean?
            {
                Fly();
                Network.SendPacket(new FlagEntityPacketOut(this, EntityFlag.FLYING, 1));
                Network.SendPacket(new FlagEntityPacketOut(this, EntityFlag.MASS, 0));
            }
            else
            {
                Unfly();
            }
            SetVelocity(Location.FromString(config.ReadString("velocity", "0,0,0")));
            Teleport(Location.FromString(config.ReadString("position", TheRegion.SpawnPoint.ToString())));
            SecureMovement = config.ReadString("secure_movement", "true").ToLowerFast() == "true"; // TODO: ReadBoolean?
            IsFirstJoin = false;
            SpawnedTime = TheRegion.GlobalTickTime;
        }

        public void SaveToYAML(YAMLConfiguration config)
        {
            config.Set("gamemode", Mode.ToString());
            config.Set("maxhealth", GetMaxHealth());
            config.Set("health", GetHealth());
            config.Set("flying", IsFlying ? "true": "false"); // TODO: Boolean safety
            config.Set("velocity", GetVelocity().ToString());
            config.Set("position", GetPosition().ToString());
            config.Set("secure_movement", SecureMovement ? "true" : "false"); // TODO: Boolean safety
            config.Set("region", TheRegion.Name);
            for (int i = 0; i < (int)NetUsageType.COUNT; i++)
            {
                string path = "stats.net_usage." + ((NetUsageType)i).ToString().ToLowerFast();
                config.Set(path, config.ReadLong(path, 0) + UsagesTotal[i]);
            }
            const string timePath = "stats.general.time_seconds";
            config.Set(timePath, config.ReadDouble(timePath, 0) + (TheRegion.GlobalTickTime - SpawnedTime));
            // TODO: Other stats!
            // TODO: CBody settings? Mass? ...?
            // TODO: Inventory!
        }

        public override EntityType GetEntityType()
        {
            return EntityType.PLAYER;
        }

        public override byte[] GetSaveBytes()
        {
            // Does not save through entity system!
            return null;
        }

        public string AnimToHold(ItemStack item)
        {
            if (item.Name == "rifle_gun")
            {
                return "torso_armed_rifle";
            }
            return "torso_armed_rifle";
            //return "idle01";
        }
        
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

        bool pkick = false;
        
        /// <summary>
        /// How far (in chunks) the player can see, as a cubic radius, excluding the chunk the player is in.
        /// </summary>
        public int ViewRadiusInChunks = 3;

        public int ViewRadExtra2 = 2;

        public int ViewRadExtra2Height = 1;

        public int ViewRadExtra5 = 3;

        public int ViewRadExtra5Height = 2;
        
        public int BestLOD = 1;

        public PhysicsEntity Manipulator_Grabbed = null;

        public ConnectorBeam Manipulator_Beam = null;

        public float Manipulator_Distance = 10;

        public Location AttemptedDirectionChange = Location.Zero;

        public Location LoadRelPos;
        public Location LoadRelDir;
        
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
            if (UsedNow != null && ((Entity)UsedNow).IsSpawned)
            {
                UsedNow.StopUse(this);
            }
            if (Network.Alive)
            {
                Network.SendMessage("Kicking you: " + message);
                Network.Alive = false;
                Network.PrimarySocket.Close(5);
            }
            // TODO: Broadcast kick message
            SysConsole.Output(OutputType.INFO, "Kicking " + this.ToString() + ": " + message);
            if (IsSpawned && !Removed)
            {
                ItemStack it = Items.GetItemForSlot(Items.cItem);
                it.Info.SwitchFrom(this, it);
                HookItem.RemoveHook(this);
                RemoveMe();
            }
            YAMLConfiguration config = new YAMLConfiguration("");
            string nl = Name.ToLower();
            string fn = "server_player_saves/" + nl[0].ToString() + "/" + nl + ".plr";
            if (Program.Files.Exists(fn))
            {
                string dat = Program.Files.ReadText(fn);
                if (dat != null)
                {
                    config = new YAMLConfiguration(dat);
                }
            }
            SaveToYAML(config);
            Program.Files.WriteText(fn, config.SaveToString());
        }

        /// <summary>
        /// The default mass of the player.
        /// </summary>
        public float tmass = 70;

        // TODO: Dictionary<breadcrumb id: int, List<Location>> ?
        public List<Location> Breadcrumbs = new List<Location>();

        public PlayerEntity(WorldSystem.Region tregion, Connection conn, string name)
            : base(tregion)
        {
            Name = name;
            model = "players/human_male_004";
            mod_zrot = 270;
            mod_scale = 1.5f;
            base.SetMaxHealth(100);
            base.SetHealth(100);
            Network = conn;
            SetMass(tmass);
            CanRotate = false;
            SetPosition(TheRegion.SpawnPoint);
            Items = new PlayerInventory(this);
            // TODO: Convert all these to item files!
            Items.GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White, "items/common/hand", true));
            Items.GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White, "items/common/fist", true));
            Items.GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White, "items/common/hook", true));
            Items.GiveItem(TheServer.Items.GetItem("admintools/manipulator", 1));
            Items.GiveItem(new ItemStack("pickaxe", TheServer, 1, "items/tools/generic_pickaxe_ico", "Generic Pickaxe", "Rapid stone mining!", Color.White, "items/tools/generic_pickaxe", false));
            Items.GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White, "items/common/flashlight", false));
            Items.GiveItem(new ItemStack("flashantilight", TheServer, 1, "items/common/flashlight_ico", "Flashantilight", "Lights things down!", Color.White, "items/common/flashlight", false));
            Items.GiveItem(new ItemStack("sun_angler", TheServer, 1, "items/tools/sun_angler", "Sun Angler", "Moves the sun itself!", Color.White, "items/tools/sun_angler", false));
            Items.GiveItem(new ItemStack("breadcrumb", TheServer, 1, "items/common/breadcrumbs", "Bread Crumbs", "Finds the way back, even over the river and through the woods!", Color.White, "items/common/breadcrumbs", false));
            for (int i = 1; i < MaterialHelpers.ALL_MATS.Count; i++)
            {
                if (MaterialHelpers.IsValid((Material)i))
                {
                    Items.GiveItem(TheServer.Items.GetItem("blocks/" + ((Material)i).GetName().ToLowerFast(), 100));
                }
            }
            Items.GiveItem(new ItemStack("pistol_gun", TheServer, 1, "items/weapons/9mm_pistol_ico", "9mm Pistol", "It shoots bullets!", Color.White, "items/weapons/silenced_pistol", false));
            Items.GiveItem(new ItemStack("shotgun_gun", TheServer, 1, "items/weapons/shotgun_ico", "Shotgun", "It shoots many bullets!", Color.White, "items/weapons/shotgun", false));
            Items.GiveItem(new ItemStack("bow", TheServer, 1, "items/weapons/bow_ico", "Bow", "It shoots arrows!", Color.White, "items/weapons/bow", false));
            Items.GiveItem(new ItemStack("explodobow", TheServer, 1, "items/weapons/explodobow_ico", "ExplodoBow", "It shoots arrows that go boom!", Color.White, "items/weapons/explodo_bow", false));
            Items.GiveItem(new ItemStack("hatcannon", TheServer, 1, "items/weapons/hatcannon_ico", "Hat Cannon", "It shoots hats!", Color.White, "items/weapons/hat_cannon", false));
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/m4"));
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/minigun"));
            Items.GiveItem(new ItemStack("suctionray", TheServer, 1, "items/tools/suctionray_ico", "Suction Ray", "Sucks things towards you!", Color.White, "items/tools/suctionray", false));
            Items.GiveItem(new ItemStack("pushray", TheServer, 1, "items/tools/pushray_ico", "Push Ray", "Pushes things away from you!", Color.White, "items/tools/pushray", false));
            Items.GiveItem(new ItemStack("bullet", "9mm_ammo", TheServer, 100, "items/weapons/ammo/9mm_round_ico", "9mm Ammo", "Nine whole millimeters!", Color.White, "items/weapons/ammo/9mm_round", false));
            Items.GiveItem(new ItemStack("bullet", "shotgun_ammo", TheServer, 100, "items/weapons/ammo/shotgun_shell_ico", "Shotgun Ammo", "Always travels in packs!", Color.White, "items/weapons/ammo/shotgun_shell", false));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", Color.White, "items/weapons/ammo/rifle_round", false));
            Items.GiveItem(new ItemStack("glowstick", TheServer, 10, "items/common/glowstick_ico", "Glowstick", "Pretty colors!", Color.Cyan, "items/common/glowstick", false));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/smoke", 10));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/smokesignal", 10));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/explosivegrenade", 10));
            Items.GiveItem(new ItemStack("smokemachine", TheServer, 10, "items/common/smokemachine_ico", "Smoke Machine", "Do not inhale!", Color.White, "items/common/smokemachine", false));
            Items.GiveItem(TheServer.Items.GetItem("special/parachute", 1));
            Items.GiveItem(TheServer.Items.GetItem("special/jetpack", 1));
            Items.GiveItem(TheServer.Items.GetItem("useful/fuel", 100));
            CGroup = CollisionUtil.Player;
            string nl = Name.ToLower();
            string fn = "server_player_saves/" + nl[0].ToString() + "/" + nl + ".plr";
            if (Program.Files.Exists(fn))
            {
                string dat = Program.Files.ReadText(fn);
                if (dat != null)
                {
                    YAMLConfiguration config = new YAMLConfiguration(dat);
                    LoadFromYAML(config);
                }
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
        
        public bool IgnoreThis(BroadPhaseEntry entry) // TODO: PhysicsEntity?
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

        public override void Fly()
        {
            if (IsFlying)
            {
                return;
            }
            base.Fly();
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.FLYING, 1));
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.MASS, 0));
        }

        public override void Unfly()
        {
            if (!IsFlying)
            {
                return;
            }
            base.Unfly();
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.FLYING, 0));
            TheRegion.SendToAll(new FlagEntityPacketOut(this, EntityFlag.MASS, PreFlyMass));
        }

        public void SetTypingStatus(bool isTyping)
        {
            IsTyping = isTyping;
            // TODO: Generic find-all-players-that-can-see-me method
            Vector3i ch = TheRegion.ChunkLocFor(GetPosition());
            SetStatusPacketOut pack = new SetStatusPacketOut(this, ClientStatus.TYPING, (byte)(IsTyping ? 1 : 0));
            foreach (PlayerEntity player in TheRegion.Players)
            {
                if (player.CanSeeChunk(ch))
                {
                    player.Network.SendPacket(pack);
                }
            }
        }

        public void GainAwarenesOf(Entity ent)
        {
            throw new NotImplementedException(); // TODO: Handle awareness-gain of entities: Set EG Status packets, etc.
        }

        public bool IsTyping = false;

        public bool IsAFK = false;
        public int TimeAFK = 0;

        public void MarkAFK()
        {
            IsAFK = true;
            TheServer.SendToAll(new MessagePacketOut("^r^7#" + Name + "^r^7 is now AFK!")); // TODO: Message configurable, localized...
            // TODO: SetStatus to all visible!
        }

        public void UnmarkAFK()
        {
            IsAFK = false;
            TheServer.SendToAll(new MessagePacketOut("^r^7#" + Name + "^r^7 is no longer AFK!")); // TODO: Message configurable, localized...
            // TODO: SetStatus to all visible!
        }

        /// <summary>
        /// Called to indicate that the player is actively doing something (IE, not AFK!)
        /// </summary>
        public void NoteDidAction()
        {
            TimeAFK = 0;
            if (IsAFK)
            {
                UnmarkAFK();
            }
        }

        public void OncePerSecondTick()
        {
            TimeAFK++;
            if (!IsAFK && TimeAFK >= 60) // TODO: Configurable timeout!
            {
                MarkAFK();
            }
        }

        public AABB Selection;

        public void NetworkSelection()
        {
            Network.SendPacket(new HighlightPacketOut(Selection));
        }

        double opstt = 0;

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
            base.Tick();
            opstt += TheRegion.Delta;
            while (opstt > 1.0)
            {
                opstt -= 1.0;
                OncePerSecondTick();
            }
            ItemStack cit = Items.GetItemForSlot(Items.cItem);
            if (GetVelocity().LengthSquared() > 1) // TODO: Move animation to CharacterEntity
            {
                // TODO: Replicate animation automation on client?
                SetAnimation("human/stand/run", 0);
                SetAnimation("human/stand/" + AnimToHold(cit), 1);
                SetAnimation("human/stand/run", 2);
            }
            else
            {
                SetAnimation("human/stand/idle01", 0);
                SetAnimation("human/stand/" + AnimToHold(cit), 1);
                SetAnimation("human/stand/idle01", 2);
            }
            if (Click) // TODO: Move clicking to CharacterEntity
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
            WasItemLefting = ItemLeft;
            WasItemUpping = ItemUp;
            WasItemRighting = ItemRight;
            Location pos = LoadRelPos;
            Vector3i cpos = TheRegion.ChunkLocFor(pos);
            ChunkMarchAndSend();
            if (cpos != pChunkLoc)
            {
                /*
                // TODO: Better system -> async?
                TrySet(pos, 1, 1, 0, 1, false);
                TrySet(pos, ViewRadiusInChunks / 2, ViewRadiusInChunks / 2, 0, 1, false);
                TrySet(pos, ViewRadiusInChunks, ViewRadiusInChunks, 0, 1, false);
                TrySet(pos, ViewRadiusInChunks + 1, ViewRadiusInChunks, 15, 2, true);
                TrySet(pos, ViewRadiusInChunks + ViewRadExtra2, ViewRadiusInChunks + ViewRadExtra2Height, 30, 2, true);
                TrySet(pos, ViewRadiusInChunks + ViewRadExtra5, ViewRadiusInChunks + ViewRadExtra5Height, 60, 5, true);
                */
                /*
                if (!loadedInitially)
                {
                    loadedInitially = true;
                    ChunkNetwork.SendPacket(new TeleportPacketOut(GetPosition()));
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 1));
                }
                else
                {
                    ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                }
                */
                foreach (ChunkAwarenessInfo ch in ChunksAwareOf.Values)
                {
                    if (!ShouldLoadChunk(ch.ChunkPos))
                    {
                        removes.Add(ch.ChunkPos);
                    }
                    else if (!ShouldSeeChunk(ch.ChunkPos) && ch.LOD <= BestLOD)
                    {
                        ch.LOD = Chunk.CHUNK_SIZE;
                    }
                }
                foreach (Vector3i loc in removes)
                {
                    Chunk ch = TheRegion.GetChunk(loc);
                    if (ch != null)
                    {
                        ForgetChunk(ch, loc);
                    }
                    else
                    {
                        ChunksAwareOf.Remove(loc);
                    }
                }
                removes.Clear();
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
            // TODO: Move use to CharacterEntity
            if (Use)
            {
                Location forw = ForwardVector();
                CollisionResult cr = TheRegion.Collision.RayTrace(GetEyePosition(), GetEyePosition() + forw * 5, IgnoreThis);
                if (cr.Hit && cr.HitEnt != null && cr.HitEnt.Tag is EntityUseable)
                {
                    if (UsedNow != (EntityUseable)cr.HitEnt.Tag)
                    {
                        if (UsedNow != null && ((Entity)UsedNow).IsSpawned)
                        {
                            UsedNow.StopUse(this);
                        }
                        UsedNow = (EntityUseable)cr.HitEnt.Tag;
                        UsedNow.StartUse(this);
                    }
                }
                else if (UsedNow != null)
                {
                    if (((Entity)UsedNow).IsSpawned)
                    {
                        UsedNow.StopUse(this);
                    }
                    UsedNow = null;
                }
            }
            else if (UsedNow != null)
            {
                if (((Entity)UsedNow).IsSpawned)
                {
                    UsedNow.StopUse(this);
                }
                UsedNow = null;
            }
            if (!CanReach(GetPosition()))
            {
                Teleport(posClamp(GetPosition()));
            }
        }

        static Vector3i[] MoveDirs = new Vector3i[] { new Vector3i(-1, 0, 0), new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, -1), new Vector3i(0, 0, 1) };

        const float Max_FOV = 90f;
        
        void ChunkMarchAndSend()
        {
            int maxChunks = TheServer.CVars.n_chunkspertick.ValueI;
            int chunksFound = 0;
            if (LoadRelPos.IsNaN() || LoadRelDir.IsNaN() || LoadRelDir.LengthSquared() < 0.1f)
            {
                return;
            }
            Matrix proj = Matrix.CreatePerspectiveFieldOfViewRH(Max_FOV * (float)Utilities.PI180, 1, 0.5f, 1000f);
            Matrix view = Matrix.CreateLookAtRH((LoadRelPos - LoadRelDir * 8).ToBVector(), (LoadRelPos + LoadRelDir * 8).ToBVector(), new Vector3(0, 0, 1));
            Matrix combined = view * proj;
            BFrustum bfs = new BFrustum(combined);
            Vector3i start = TheRegion.ChunkLocFor(GetEyePosition());
            HashSet<Vector3i> seen = new HashSet<Vector3i>();
            Queue<Vector3i> toSee = new Queue<Vector3i>();
            toSee.Enqueue(start);
            while (toSee.Count > 0)
            {
                Vector3i cur = toSee.Dequeue();
                seen.Add(cur);
                if (Math.Abs(cur.X - start.X) > (ViewRadiusInChunks + ViewRadExtra5)
                    || Math.Abs(cur.Y - start.Y) > (ViewRadiusInChunks + ViewRadExtra5)
                    || Math.Abs(cur.Z - start.Z) > (ViewRadiusInChunks + ViewRadExtra5Height))
                {
                    continue;
                }
                if (Math.Abs(cur.X - start.X) <= ViewRadiusInChunks
                    || Math.Abs(cur.Y - start.Y) <= ViewRadiusInChunks
                    || Math.Abs(cur.Z - start.Z) <= ViewRadiusInChunks)
                {
                    if (TryChunk(cur, 0, 1))
                    {
                        chunksFound++;
                        if (chunksFound > maxChunks)
                        {
                            return;
                        }
                    }
                }
                else if (Math.Abs(cur.X - start.X) <= (ViewRadiusInChunks + ViewRadExtra2)
                    || Math.Abs(cur.Y - start.Y) <= (ViewRadiusInChunks + ViewRadExtra2)
                    || Math.Abs(cur.Z - start.Z) <= (ViewRadiusInChunks + ViewRadExtra2Height))
                {
                    if (TryChunk(cur, 10, 2))
                    {
                        chunksFound++;
                        if (chunksFound > maxChunks)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (TryChunk(cur, 25, 5))
                    {
                        chunksFound++;
                        if (chunksFound > maxChunks)
                        {
                            return;
                        }
                    }
                }
                for (int i = 0; i < MoveDirs.Length; i++)
                {
                    Vector3i t = cur + MoveDirs[i];
                    if (!seen.Contains(t) && !toSee.Contains(t))
                    {
                        //toSee.Enqueue(t);
                        for (int j = 0; j < MoveDirs.Length; j++)
                        {
                            if (Vector3.Dot(MoveDirs[j].ToVector3(), LoadRelDir.ToBVector()) < -0.8f) // TODO: Wut?
                            {
                                continue;
                            }
                            Vector3i nt = cur + MoveDirs[j];
                            if (!seen.Contains(nt) && !toSee.Contains(nt))
                            {
                                bool val = false;
                                Chunk ch = TheRegion.GetChunk(t);
                                if (ch == null)
                                {
                                    val = true;
                                }
                                // TODO: Oh, come on!
                                else if (MoveDirs[i].X == -1)
                                {
                                    if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_XM];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XP];
                                    }
                                }
                                else if (MoveDirs[i].X == 1)
                                {
                                    if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_XM];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XM];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XM];
                                    }
                                }
                                else if (MoveDirs[i].Y == -1)
                                {
                                    if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.YP_YM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Y == 1)
                                {
                                    if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.YP_YM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XM_YP];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.XP_YP];
                                    }
                                    else if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                    else if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Z == -1)
                                {
                                    if (MoveDirs[j].Z == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_ZM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XM];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_XP];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_YP];
                                    }
                                }
                                else if (MoveDirs[i].Z == 1)
                                {
                                    if (MoveDirs[j].Z == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZP_ZM];
                                    }
                                    else if (MoveDirs[j].X == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XM];
                                    }
                                    else if (MoveDirs[j].X == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_XP];
                                    }
                                    else if (MoveDirs[j].Y == -1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YM];
                                    }
                                    else if (MoveDirs[j].Y == 1)
                                    {
                                        val = ch.Reachability[(int)ChunkReachability.ZM_YP];
                                    }
                                }
                                if (val)
                                {
                                    Location min = nt.ToLocation() * Chunk.CHUNK_SIZE;
                                    if (bfs.ContainsBox(min, min + new Location(Chunk.CHUNK_SIZE)))
                                    {
                                        toSee.Enqueue(nt);
                                    }
                                    else
                                    {
                                        seen.Add(nt);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public Location posClamp(Location pos)
        {
            int maxdist = Math.Abs(TheServer.CVars.g_maxdist.ValueI);
            pos.X = Clamp(pos.X, -maxdist, maxdist);
            pos.Y = Clamp(pos.Y, -maxdist, maxdist);
            pos.Z = Clamp(pos.Z, -maxdist, maxdist);
            return pos;
        }

        public double Clamp(double num, double min, double max)
        {
            if (num < min)
            {
                return min;
            }
            else if (num > max)
            {
                return max;
            }
            else
            {
                return num;
            }
        }

        List<Vector3i> removes = new List<Vector3i>();

        public Location losPos = Location.NaN;

        public bool ShouldLoadChunk(Vector3i cpos)
        {
            Vector3i wpos = TheRegion.ChunkLocFor(LoadRelPos);
            if (Math.Abs(cpos.X - wpos.X) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Y - wpos.Y) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Z - wpos.Z) > (ViewRadiusInChunks + ViewRadExtra5Height))
            {
                return false;
            }
            return true;
        }

        public bool ShouldLoadChunkPreviously(Vector3i cpos)
        {
            if (lPos.IsNaN())
            {
                return false;
            }
            Vector3i wpos = TheRegion.ChunkLocFor(lPos);
            if (Math.Abs(cpos.X - wpos.X) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Y - wpos.Y) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Z - wpos.Z) > (ViewRadiusInChunks + ViewRadExtra5Height))
            {
                return false;
            }
            return true;
        }

        public bool ShouldSeeLODChunkOneSecondAgo(Vector3i cpos)
        {
            Vector3i wpos = TheRegion.ChunkLocFor(losPos);
            if (Math.Abs(cpos.X - wpos.X) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Y - wpos.Y) > (ViewRadiusInChunks + ViewRadExtra5)
                || Math.Abs(cpos.Z - wpos.Z) > (ViewRadiusInChunks + ViewRadExtra5Height))
            {
                return false;
            }
            return true;
        }

        public bool ShouldSeeChunkOneSecondAgo(Vector3i cpos)
        {
            if (losPos.IsNaN())
            {
                return false;
            }
            Vector3i wpos = TheRegion.ChunkLocFor(losPos);
            if (Math.Abs(cpos.X - wpos.X) > ViewRadiusInChunks
                || Math.Abs(cpos.Y - wpos.Y) > ViewRadiusInChunks
                || Math.Abs(cpos.Z - wpos.Z) > ViewRadiusInChunks)
            {
                return false;
            }
            return true;
        }

        public bool ShouldSeeChunkPreviously(Vector3i cpos)
        {
            if (lPos.IsNaN())
            {
                return false;
            }
            Vector3i wpos = TheRegion.ChunkLocFor(lPos);
            if (Math.Abs(cpos.X - wpos.X) > ViewRadiusInChunks
                || Math.Abs(cpos.Y - wpos.Y) > ViewRadiusInChunks
                || Math.Abs(cpos.Z - wpos.Z) > ViewRadiusInChunks)
            {
                return false;
            }
            return true;
        }

        public bool ShouldSeeChunk(Vector3i cpos)
        {
            if (LoadRelPos.IsNaN())
            {
                return false;
            }
            Vector3i wpos = TheRegion.ChunkLocFor(LoadRelPos);
            if (Math.Abs(cpos.X - wpos.X) > ViewRadiusInChunks
                || Math.Abs(cpos.Y - wpos.Y) > ViewRadiusInChunks
                || Math.Abs(cpos.Z - wpos.Z) > ViewRadiusInChunks)
            {
                return false;
            }
            return true;
        }

        public bool ShouldSeePositionOneSecondAgo(Location pos)
        {
            if (pos.IsNaN() || losPos.IsNaN())
            {
                return false;
            }
            return ShouldSeeChunkOneSecondAgo(TheRegion.ChunkLocFor(pos));
        }

        public bool ShouldSeeLODPositionOneSecondAgo(Location pos)
        {
            if (pos.IsNaN() || losPos.IsNaN())
            {
                return false;
            }
            return ShouldSeeLODChunkOneSecondAgo(TheRegion.ChunkLocFor(pos));
        }

        public bool ShouldSeePositionPreviously(Location pos)
        {
            if (pos.IsNaN() || lPos.IsNaN())
            {
                return false;
            }
            return ShouldSeeChunkPreviously(TheRegion.ChunkLocFor(pos));
        }

        public bool ShouldSeePosition(Location pos)
        {
            if (pos.IsNaN())
            {
                return false;
            }
            return ShouldSeeChunk(TheRegion.ChunkLocFor(pos));
        }

        public bool ShouldLoadPosition(Location pos)
        {
            if (pos.IsNaN())
            {
                return false;
            }
            return ShouldLoadChunk(TheRegion.ChunkLocFor(pos));
        }

        public bool ShouldLoadPositionPreviously(Location pos)
        {
            if (pos.IsNaN())
            {
                return false;
            }
            return ShouldLoadChunkPreviously(TheRegion.ChunkLocFor(pos));
        }

        public int BreadcrumbRadius = 6;

        Vector3i pChunkLoc = new Vector3i(-100000, -100000, -100000);
        
       // bool loadedInitially = false;

        /*
        public void TrySet(Location pos, int VIEWRAD_HOR, int VIEWRAD_VERT, float atime, int posMult, bool bg)
        {
            for (int x = -VIEWRAD_HOR; x <= VIEWRAD_HOR; x++)
            {
                for (int y = -VIEWRAD_HOR; y <= VIEWRAD_HOR; y++)
                {
                    for (int z = -VIEWRAD_VERT; z <= VIEWRAD_VERT; z++)
                    {
                        if (bg)
                        {
                            Vector3i chl = TheRegion.ChunkLocFor(pos + new Location(Chunk.CHUNK_SIZE * x, Chunk.CHUNK_SIZE * y, Chunk.CHUNK_SIZE * z));
                            TheRegion.TheServer.Schedule.ScheduleSyncTask(() =>
                            {
                                if (pkick)
                                {
                                    return;
                                }
                                if (posMult == 5)
                                {
                                    Chunk ch = TheRegion.LoadChunkLOD(chl);
                                    if (ch != null)
                                    {
                                        TryChunk(chl.ToLocation() * Chunk.CHUNK_SIZE, 0, posMult, ch);
                                        ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                                        return;
                                    }
                                }
                                TheRegion.LoadChunk_Background(chl, (b) =>
                                {
                                    TryChunk(chl.ToLocation() * Chunk.CHUNK_SIZE, 0, posMult);
                                    if (!pkick)
                                    {
                                        ChunkNetwork.SendPacket(new OperationStatusPacketOut(StatusOperation.CHUNK_LOAD, 2));
                                    }
                                });
                            }, Utilities.UtilRandom.NextDouble() * atime);
                        }
                        else
                        {
                            TryChunk(pos + new Location(Chunk.CHUNK_SIZE * x, Chunk.CHUNK_SIZE * y, Chunk.CHUNK_SIZE * z), atime, posMult);
                        }
                    }
                }
            }
        }*/

        public bool TryChunk(Vector3i cworldPos, float atime, int posMult, Chunk chi = null) // TODO: Efficiency?
        {
            if (pkick)
            {
                return false;
            }
            //Vector3i cworldPos = TheRegion.ChunkLocFor(worldPos);
            if (!ChunksAwareOf.ContainsKey(cworldPos) || ChunksAwareOf[cworldPos].LOD > posMult) // TODO: Efficiency - TryGetValue?
            { // TODO: Or ATime < awareOf.remTime?
                if (ChunksAwareOf.ContainsKey(cworldPos)) // TODO: Efficiency - TryGetValue?
                {
                    ChunkAwarenessInfo acai = ChunksAwareOf[cworldPos];
                    if (acai != null && acai.SendToClient != null && acai.SendToClient.Time > 0)
                    {
                        TheServer.Schedule.DescheduleSyncTask(acai.SendToClient);
                    }
                }
                if (atime == 0)
                {
                    Chunk chk = chi != null ? chi : TheRegion.LoadChunk(cworldPos);
                    ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk, posMult));
                    ChunksAwareOf.Remove(cworldPos);
                    ChunksAwareOf.Add(cworldPos, new ChunkAwarenessInfo() { ChunkPos = cworldPos, LOD = posMult, SendToClient = null });
                }
                else
                {
                    SyncScheduleItem item = TheServer.Schedule.ScheduleSyncTask(() =>
                    {
                        if (!pkick)
                        {
                            Chunk chk = TheRegion.LoadChunk(cworldPos);
                            ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk, posMult));
                        }
                    }, Utilities.UtilRandom.NextDouble() * atime);
                    ChunksAwareOf.Remove(cworldPos);
                    ChunksAwareOf.Add(cworldPos, new ChunkAwarenessInfo() { ChunkPos = cworldPos, LOD = posMult, SendToClient = item });
                }
                return true;
            }
            return false;
        }

        public Dictionary<Vector3i, ChunkAwarenessInfo> ChunksAwareOf = new Dictionary<Vector3i, ChunkAwarenessInfo>();

        public bool CanSeeChunk(Vector3i cpos)
        {
            return ChunksAwareOf.ContainsKey(cpos);
        }

        public override void EndTick()
        {
            if (UpdateLoadPos)
            {
                base.EndTick();
                LoadRelPos = lPos;
                LoadRelDir = ForwardVector();
            }
            else
            {
                lPos = LoadRelPos;
            }
        }

        public bool ForgetChunk(Chunk ch, Vector3i cpos)
        {
            if (ChunksAwareOf.Remove(cpos))
            {
                foreach (Entity ent in TheRegion.Entities)
                {
                    if (ch.Contains(ent.GetPosition()))
                    {
                        Network.SendPacket(new DespawnEntityPacketOut(ent.EID));
                    }
                }
                ChunkNetwork.SendPacket(new ChunkForgetPacketOut(cpos));
                return true;
            }
            return false;
        }

        public bool CanReach(Location pos)
        {
            int maxdist = Math.Abs(TheServer.CVars.g_maxdist.ValueI);
            return Math.Abs(pos.X) < maxdist && Math.Abs(pos.Y) < maxdist && Math.Abs(pos.Z) < maxdist;
        }

        public bool UpdateLoadPos = true;

        public override void SetPosition(Location pos)
        {
            Location l = posClamp(pos);
            if (UpdateLoadPos)
            {
                LoadRelPos = l;
                LoadRelDir = ForwardVector();
            }
            base.SetPosition(l);
        }

        public void Teleport(Location pos)
        {
            SetPosition(pos);
            Network.SendPacket(new TeleportPacketOut(GetPosition()));
        }

        public override string ToString()
        {
            return Name;
        }

        public void SendStatus()
        {
            Network.SendPacket(new YourStatusPacketOut(GetHealth(), GetMaxHealth(), Flags));
        }

        public override void SetHealth(float health)
        {
            base.SetHealth(health);
            SendStatus();
        }

        public override void SetMaxHealth(float maxhealth)
        {
            base.SetMaxHealth(maxhealth);
            SendStatus();
        }

        public override void Die()
        {
            SetHealth(MaxHealth);
            Teleport(TheRegion.SpawnPoint);
        }

        public BlockGroupEntity Pasting = null;

        public float PastingDist = 5;
    }

    public class ChunkAwarenessInfo
    {
        public Vector3i ChunkPos;

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
