//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
using LiteDB;
using FreneticDataSyntax;

namespace Voxalia.ServerGame.EntitySystem
{
    public class PlayerEntity: HumanoidEntity
    {
        public string SessionKey = null;

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.CHARACTER;
        }

        public override byte[] GetNetData()
        {
            return GetCharacterNetData();
        }

        public GameMode Mode = GameMode.SURVIVOR;

        public bool IsFirstJoin = true;

        public long[] UsagesTotal = new long[(int)NetUsageType.COUNT];

        public double SpawnedTime = 0;

        public bool SecureMovement = true;

        public FDSSection Permissions = new FDSSection();

        public void SendLanguageData(TextChannel channel, params string[] message)
        {
            Network.SendLanguageData(channel, message);
        }

        public void SendMessage(TextChannel channel, string message)
        {
            Network.SendMessage(channel, message);
        }
        
        public void LoadFromSaves(FDSSection config)
        {
            string world = config.GetString("world", null);
            if (world != null) // TODO: && worldIsValidAndLoaded
            {
                // TODO: Set world!
            }
            if (!Enum.TryParse(config.GetString("gamemode", "SURVIVOR"), out Mode))
            {
                SysConsole.Output(OutputType.WARNING, "Invalid gamemode for " + Name + ", reverting to SURVIVOR!");
                Mode = GameMode.SURVIVOR;
            }
            base.SetMaxHealth(config.GetFloat("maxhealth", 100).Value);
            base.SetHealth(config.GetFloat("health", 100).Value);
            if (config.GetString("flying", "false").ToLowerFast() == "true") // TODO: ReadBoolean?
            {
                TheRegion.TheWorld.Schedule.ScheduleSyncTask(() =>
                {
                    Fly();
                }, 0.1);
            }
            SetVelocity(Location.FromString(config.GetString("velocity", "0,0,0")));
            SetPosition(Location.FromString(config.GetString("position", TheRegion.TheWorld.SpawnPoint.ToString())));
            SecureMovement = config.GetString("secure_movement", "true").ToLowerFast() == "true"; // TODO: ReadBoolean?
            if (config.HasKey("permissions"))
            {
                Permissions = config.GetSection("permissions");
            }
            if (Permissions == null)
            {
                Permissions = new FDSSection();
            }
            EID = config.GetLong("eid").Value;
            IsFirstJoin = false;
            SpawnedTime = TheRegion.GlobalTickTime;
        }

        public void SaveToYAML(FDSSection config)
        {
            config.Set("gamemode", Mode.ToString());
            config.Set("maxhealth", GetMaxHealth());
            config.Set("health", GetHealth());
            config.Set("flying", IsFlying ? "true": "false"); // TODO: Boolean safety
            config.Set("velocity", GetVelocity().ToString());
            config.Set("position", GetPosition().ToString());
            config.Set("secure_movement", SecureMovement ? "true" : "false"); // TODO: Boolean safety
            config.Set("world", TheRegion.TheWorld.Name);
            for (int i = 0; i < (int)NetUsageType.COUNT; i++)
            {
                string path = "stats.net_usage." + ((NetUsageType)i).ToString().ToLowerFast();
                config.Set(path, config.GetLong(path, 0).Value + UsagesTotal[i]);
            }
            const string timePath = "stats.general.time_seconds";
            config.Set(timePath, config.GetDouble(timePath, 0).Value + (TheRegion.GlobalTickTime - SpawnedTime));
            config.Set("permissions", Permissions);
            config.Set("eid", EID);
            // TODO: Other stats!
            // TODO: CBody settings? Mass? ...?
            // TODO: Inventory!
        }

        public bool HasSpecificPermissionNodeInternal(string node)
        {
            return Permissions.GetString(node, "false").ToLowerFast() == "true";
        }

        public bool HasPermissionInternal(params string[] path)
        {
            string constructed = "";
            for (int i = 0; i < path.Length; i++)
            {
                if (HasSpecificPermissionNodeInternal(constructed + "*"))
                {
                    return true;
                }
                constructed += path[i] + ".";
            }
            return HasSpecificPermissionNodeInternal(constructed.Substring(0, constructed.Length - 1));
        }

        public bool HasPermission(string permission)
        {
            return HasPermissionInternal(permission.ToLowerFast().SplitFast('.'));
        }

        public override EntityType GetEntityType()
        {
            return EntityType.PLAYER;
        }
        
        public override BsonDocument GetSaveData()
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

        public int ViewRadExtra2 = 1;

        public int ViewRadExtra2Height = 1;

        public int ViewRadExtra5 = 1;

        public int ViewRadExtra5Height = 1;
        
        public int BestLOD = 1;

        public PhysicsEntity Manipulator_Grabbed = null;

        public ConnectorBeam Manipulator_Beam = null;

        public double Manipulator_Distance = 10;

        public Location AttemptedDirectionChange = Location.Zero;

        public Location LoadRelPos;
        public Location LoadRelDir;

        public FDSSection PlayerConfig = null;
        
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
                SendMessage(TextChannel.IMPORTANT, "Kicking you: " + message);
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
            string nl = Name.ToLower();
            string fn = "server_player_saves/" + nl[0].ToString() + "/" + nl + ".plr";
            SaveToYAML(PlayerConfig);
            TheServer.Files.WriteText(fn, PlayerConfig.SaveToString());
        }

        /// <summary>
        /// The default mass of the player.
        /// </summary>
        public double tmass = 70;

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
            SetPosition(TheRegion.TheWorld.SpawnPoint);
            Items = new PlayerInventory(this);
        }

        public void InitPlayer()
        {
            // TODO: Convert all these to item files!
            Items.GiveItem(new ItemStack("open_hand", TheServer, 1, "items/common/open_hand_ico", "Open Hand", "Grab things!", Color.White, "items/common/hand", true, 0));
            Items.GiveItem(new ItemStack("fist", TheServer, 1, "items/common/fist_ico", "Fist", "Hit things!", Color.White, "items/common/fist", true, 0));
            Items.GiveItem(new ItemStack("hook", TheServer, 1, "items/common/hook_ico", "Grappling Hook", "Grab distant things!", Color.White, "items/common/hook", true, 0));
            Items.GiveItem(TheServer.Items.GetItem("admintools/manipulator", 1));
            Items.GiveItem(new ItemStack("pickaxe", TheServer, 1, "render_model:self", "Generic Pickaxe", "Rapid stone mining!", Color.White, "items/tools/generic_pickaxe", false, 0));
            Items.GiveItem(new ItemStack("flashlight", TheServer, 1, "items/common/flashlight_ico", "Flashlight", "Lights things up!", Color.White, "items/common/flashlight", false, 0));
            Items.GiveItem(new ItemStack("flashantilight", TheServer, 1, "items/common/flashlight_ico", "Flashantilight", "Lights things down!", Color.White, "items/common/flashlight", false, 0));
            Items.GiveItem(new ItemStack("sun_angler", TheServer, 1, "items/tools/sun_angler", "Sun Angler", "Moves the sun itself!", Color.White, "items/tools/sun_angler", false, 0));
            Items.GiveItem(new ItemStack("breadcrumb", TheServer, 1, "items/common/breadcrumbs", "Bread Crumbs", "Finds the way back, even over the river and through the woods!", Color.White, "items/common/breadcrumbs", false, 0));
            for (int i = 1; i < MaterialHelpers.ALL_MATS.Count; i++)
            {
                if (MaterialHelpers.IsValid((Material)i))
                {
                    Items.GiveItem(TheServer.Items.GetItem("blocks/" + ((Material)i).GetName().ToLowerFast(), 100));
                }
            }
            Items.GiveItem(new ItemStack("pistol_gun", TheServer, 1, "render_model:self", "9mm Pistol", "It shoots bullets!", Color.White, "items/weapons/silenced_pistol", false, 0));
            Items.GiveItem(new ItemStack("shotgun_gun", TheServer, 1, "items/weapons/shotgun_ico", "Shotgun", "It shoots many bullets!", Color.White, "items/weapons/shotgun", false, 0));
            Items.GiveItem(new ItemStack("bow", TheServer, 1, "items/weapons/bow_ico", "Bow", "It shoots arrows!", Color.White, "items/weapons/bow", false, 0));
            Items.GiveItem(new ItemStack("explodobow", TheServer, 1, "items/weapons/explodobow_ico", "ExplodoBow", "It shoots arrows that go boom!", Color.White, "items/weapons/explodo_bow", false, 0));
            Items.GiveItem(new ItemStack("hatcannon", TheServer, 1, "items/weapons/hatcannon_ico", "Hat Cannon", "It shoots hats!", Color.White, "items/weapons/hat_cannon", false, 0));
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/m4"));
            Items.GiveItem(TheServer.Items.GetItem("weapons/rifles/minigun"));
            Items.GiveItem(new ItemStack("suctionray", TheServer, 1, "items/tools/suctionray_ico", "Suction Ray", "Sucks things towards you!", Color.White, "items/tools/suctionray", false, 0));
            Items.GiveItem(new ItemStack("pushray", TheServer, 1, "items/tools/pushray_ico", "Push Ray", "Pushes things away from you!", Color.White, "items/tools/pushray", false, 0));
            Items.GiveItem(new ItemStack("bullet", "9mm_ammo", TheServer, 100, "items/weapons/ammo/9mm_round_ico", "9mm Ammo", "Nine whole millimeters!", Color.White, "items/weapons/ammo/9mm_round", false, 0));
            Items.GiveItem(new ItemStack("bullet", "shotgun_ammo", TheServer, 100, "items/weapons/ammo/shotgun_shell_ico", "Shotgun Ammo", "Always travels in packs!", Color.White, "items/weapons/ammo/shotgun_shell", false, 0));
            Items.GiveItem(new ItemStack("bullet", "rifle_ammo", TheServer, 1000, "items/weapons/ammo/rifle_round_ico", "Assault Rifle Ammo", "Very rapid!", Color.White, "items/weapons/ammo/rifle_round", false, 0));
            Items.GiveItem(new ItemStack("glowstick", TheServer, 10, "items/common/glowstick_ico", "Glowstick", "Pretty colors!", Color.Cyan, "items/common/glowstick", false, 0));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/smoke", 10));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/smokesignal", 10));
            Items.GiveItem(TheServer.Items.GetItem("weapons/grenades/explosivegrenade", 10));
            Items.GiveItem(new ItemStack("smokemachine", TheServer, 10, "items/common/smokemachine_ico", "Smoke Machine", "Do not inhale!", Color.White, "items/common/smokemachine", false, 0));
            Items.GiveItem(TheServer.Items.GetItem("special/parachute", 1));
            Items.GiveItem(TheServer.Items.GetItem("special/jetpack", 1));
            Items.GiveItem(TheServer.Items.GetItem("useful/fuel", 100));
            Items.GiveItem(TheServer.Items.GetItem("special/wings", 1));
            CGroup = CollisionUtil.Player;
            string nl = Name.ToLower();
            string fn = "server_player_saves/" + nl[0].ToString() + "/" + nl + ".plr";
            if (TheServer.Files.Exists(fn))
            {
                // TODO: Journaling read
                // TODO: Use ServerBase.GetPlayerConfig() ?
                string dat = TheServer.Files.ReadText(fn);
                if (dat != null)
                {
                    PlayerConfig = new FDSSection(dat);
                    LoadFromSaves(PlayerConfig);
                }
            }
            if (PlayerConfig == null)
            {
                PlayerConfig = new FDSSection();
                SaveToYAML(PlayerConfig);
            }
        }

        public override void Solidify()
        {
            Flags |= YourStatusFlags.NON_SOLID;
            SendStatus();
            base.Solidify();
        }

        public override void Desolidify()
        {
            Flags &= ~YourStatusFlags.NON_SOLID;
            SendStatus();
            base.Desolidify();
        }

        public void SetAnimation(string anim, byte mode)
        {
            if (mode == 0)
            {
                if (hAnim != null && hAnim.Name == anim)
                {
                    return;
                }
                hAnim = TheServer.Animations.GetAnimation(anim, TheServer.Files);
            }
            else if (mode == 1)
            {
                if (tAnim != null && tAnim.Name == anim)
                {
                    return;
                }
                tAnim = TheServer.Animations.GetAnimation(anim, TheServer.Files);
            }
            else
            {
                if (lAnim != null && lAnim.Name == anim)
                {
                    return;
                }
                lAnim = TheServer.Animations.GetAnimation(anim, TheServer.Files);
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
            TheServer.Broadcast("^r^7#" + Name + "^r^7 is now AFK!"); // TODO: Message configurable, localized...
            // TODO: SetStatus to all visible!
        }

        public void UnmarkAFK()
        {
            IsAFK = false;
            TheServer.Broadcast("^r^7#" + Name + "^r^7 is no longer AFK!"); // TODO: Message configurable, localized...
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
            if (cpos != pChunkLoc)
            {
                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        for (int z = -2; z <= 2; z++)
                        {
                            TryChunk(cpos + new Vector3i(x, y, z), 1);
                        }
                    }
                }
            }
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

        const double Max_FOV = 100f;
        
        void ChunkMarchAndSend()
        {
            // TODO: is this the most efficient it can be?
            int maxChunks = TheServer.CVars.n_chunkspertick.ValueI;
            int chunksFound = 0;
            if (LoadRelPos.IsNaN() || LoadRelDir.IsNaN() || LoadRelDir.LengthSquared() < 0.1f)
            {
                return;
            }
            Matrix proj = Matrix.CreatePerspectiveFieldOfViewRH(Max_FOV * (double)Utilities.PI180, 1, 0.5f, 3000f);
            Matrix view = Matrix.CreateLookAtRH((LoadRelPos - LoadRelDir * 8).ToBVector(), (LoadRelPos + LoadRelDir * 8).ToBVector(), new Vector3(0, 0, 1));
            Matrix combined = view * proj;
            BFrustum bfs = new BFrustum(combined);
            Vector3i start = TheRegion.ChunkLocFor(LoadRelPos);
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
                    && Math.Abs(cur.Y - start.Y) <= ViewRadiusInChunks
                    && Math.Abs(cur.Z - start.Z) <= ViewRadiusInChunks)
                {
                    if (TryChunk(cur, 1))
                    {
                        chunksFound++;
                        if (chunksFound > maxChunks)
                        {
                            return;
                        }
                    }
                }
                else if (Math.Abs(cur.X - start.X) <= (ViewRadiusInChunks + ViewRadExtra2)
                    && Math.Abs(cur.Y - start.Y) <= (ViewRadiusInChunks + ViewRadExtra2)
                    && Math.Abs(cur.Z - start.Z) <= (ViewRadiusInChunks + ViewRadExtra2Height))
                {
                    if (TryChunk(cur, 2))
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
                    if (TryChunk(cur, 5))
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
            double maxdist = Math.Abs(TheServer.CVars.g_maxdist.ValueD);
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
            if (pos.IsNaN())
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
        
        public bool TryChunk(Vector3i cworldPos, int posMult, Chunk chi = null) // TODO: Efficiency?
        {
            if (pkick)
            {
                return false;
            }
            if (!ChunksAwareOf.ContainsKey(cworldPos) || ChunksAwareOf[cworldPos].LOD > posMult) // TODO: Efficiency - TryGetValue?
            {
                double dist = (cworldPos.ToLocation() * Chunk.CHUNK_SIZE - LoadRelPos).LengthSquared();
                bool async = chi == null && dist > (Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 2 * 2);
                if (async)
                {
                    TheRegion.LoadChunk_Background(cworldPos, (chn) =>
                    {
                        if (!pkick && chn != null)
                        {
                            ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chn, posMult));
                        }
                    });
                }
                else
                {
                    Chunk chk = chi != null ? chi : TheRegion.LoadChunk(cworldPos);
                    ChunkNetwork.SendPacket(new ChunkInfoPacketOut(chk, posMult));
                }
                ChunksAwareOf.Remove(cworldPos);
                ChunksAwareOf.Add(cworldPos, new ChunkAwarenessInfo() { ChunkPos = cworldPos, LOD = posMult });
                return true;
            }
            return false;
        }

        public Dictionary<Vector3i, ChunkAwarenessInfo> ChunksAwareOf = new Dictionary<Vector3i, ChunkAwarenessInfo>();

        public bool CanSeeChunk(Vector3i cpos)
        {
            return ChunksAwareOf.ContainsKey(cpos);
        }

        /// <summary>
        /// This is a temporary lazy way of tracking known entities to prevent double-spawning.
        /// </summary>
        public HashSet<long> Known = new HashSet<long>(); // TODO: Scrap this, use CanSeePreviously, etc.

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
            double maxdist = Math.Abs(TheServer.CVars.g_maxdist.ValueD);
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

        public override void SetHealth(double health)
        {
            base.SetHealth(health);
            SendStatus();
        }

        public override void SetMaxHealth(double maxhealth)
        {
            base.SetMaxHealth(maxhealth);
            SendStatus();
        }

        public override void Die()
        {
            SetHealth(MaxHealth);
            Teleport(TheRegion.TheWorld.SpawnPoint);
        }

        public BlockGroupEntity Pasting = null;

        public double PastingDist = 5;

        public PlaneEntity Wings = null;
    }

    public class ChunkAwarenessInfo
    {
        public Vector3i ChunkPos;

        public int LOD;

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
