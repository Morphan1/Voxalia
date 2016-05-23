using System;
using System.Collections.Generic;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities.Threading;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using FreneticScript;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        /// <summary>
        /// How much time has elapsed since the last tick started.
        /// </summary>
        public double Delta;

        public double GlobalTickTime = 0;

        public ChunkDataManager ChunkManager;

        public void ChunkSendToAll(AbstractPacketOut packet, Location cpos)
        {
            if (cpos.IsNaN())
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    Players[i].Network.SendPacket(packet);
                }
            }
            else
            {
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i].CanSeeChunk(cpos))
                    {
                        Players[i].Network.SendPacket(packet);
                    }
                }
            }
        }

        public void SendToAll(AbstractPacketOut packet)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Network.SendPacket(packet);
            }
        }

        public void Broadcast(string message)
        {
            SysConsole.Output(OutputType.INFO, "[Broadcast] " + message);
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Network.SendMessage(message);
            }
        }

        public int MaxViewRadiusInChunks = 4;

        public YAMLConfiguration Config;

        Thread MainThread;

        int MainThreadID;

        public void CheckThreadValidity()
        {
            if (Thread.CurrentThread.ManagedThreadId != MainThreadID)
            {
                throw new Exception("Called a critical method on the wrong thread!");
            }
        }

        public void BuildWorld()
        {
            MainThread = Thread.CurrentThread;
            MainThreadID = MainThread.ManagedThreadId;
            ParallelLooper pl = new ParallelLooper();
            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                pl.AddThread();
            }
            CollisionDetectionSettings.AllowedPenetration = 0.01f;
            PhysicsWorld = new Space(pl);
            PhysicsWorld.TimeStepSettings.MaximumTimeStepsPerFrame = 10;
            PhysicsWorld.ForceUpdater.Gravity = new Vector3(0, 0, -9.8f * 3f / 2f);
            PhysicsWorld.DuringForcesUpdateables.Add(new LiquidVolume(this));
            Collision = new CollisionUtil(PhysicsWorld);
            string folder = "saves/" + Name;
            Program.Files.CreateDirectory(folder);
            string fname = folder + "/region.yml";
            if (Program.Files.Exists(fname))
            {
                Config = new YAMLConfiguration(Program.Files.ReadText(fname));
            }
            else
            {
                Config = new YAMLConfiguration("");
            }
            Config.Changed += configChanged;
            Config.Set("general.IMPORTANT_NOTE", "Edit this configuration at your own risk!");
            Config.Set("general.name", Name);
            Config.Default("general.seed", Utilities.UtilRandom.Next(SeedMax) - SeedMax / 2);
            Config.Default("general.spawnpoint", new Location(0, 0, 50).ToString());
            CFGEdited = true;
            Seed = Config.ReadInt("general.seed", 100);
            SpawnPoint = Location.FromString(Config.ReadString("general.spawnpoint", "0,0,50"));
            Random seedGen = new Random(Seed);
            Seed2 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed3 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed4 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed5 = (seedGen.Next(SeedMax) - SeedMax / 2);
            EntityConstructors.Add(EntityType.ITEM, new ItemEntityConstructor());
            EntityConstructors.Add(EntityType.BLOCK_ITEM, new BlockItemEntityConstructor());
            EntityConstructors.Add(EntityType.GLOWSTICK, new GlowstickEntityConstructor());
            EntityConstructors.Add(EntityType.MODEL, new ModelEntityConstructor());
            EntityConstructors.Add(EntityType.SMOKE_GRENADE, new SmokeGrenadeEntityConstructor());
            EntityConstructors.Add(EntityType.MUSIC_BLOCK, new MusicBlockEntityConstructor());
            ChunkManager = new ChunkDataManager();
            ChunkManager.Init(this);
            //LoadRegion(new Location(-MaxViewRadiusInChunks * 30), new Location(MaxViewRadiusInChunks * 30), true);
            //TheServer.Schedule.RunAllSyncTasks(0.016); // TODO: Separate per-region scheduler // Also don't freeze the entire server/region just because we're waiting on chunks >.>
            //SysConsole.Output(OutputType.INIT, "Finished building chunks! Now have " + LoadedChunks.Count + " chunks!");
        }

        public Location SpawnPoint;

        const int SeedMax = ushort.MaxValue;

        bool CFGEdited = false;

        void configChanged(int prio, EventArgs e)
        {
            CFGEdited = true;
        }

        public void LoadRegion(Location min, Location max, bool announce = true)
        {
            Location minc = ChunkLocFor(min);
            Location maxc = ChunkLocFor(max);
            Object locker = new Object();
            int c = 0;
            int done = 0;
            for (double x = minc.X; x <= maxc.X; x++)
            {
                for (double y = minc.Y; y <= maxc.Y; y++)
                {
                    for (double z = minc.Z; z <= maxc.Z; z++)
                    {
                        LoadChunk_Background_Startup(new Location(x, y, z), (o) => { lock (locker) { done++; } }, LoadTimeScheduler);
                        c++;
                    }
                }
            }
            bool cont = true;
            double time = 0;
            while (cont)
            {
                LoadTimeScheduler.RunAllSyncTasks(0.016);
                lock (locker)
                {
                    cont = done < c;
                    time += 0.016;
                    if (time > 1)
                    {
                        time -= 1;
                        SysConsole.Output(OutputType.INFO, "Loaded " + done + "/" + c + " chunks so far!");
                    }
                }
                Thread.Sleep(16);
            }
            LoadTimeScheduler.RunAllSyncTasks(0.016);
            TheServer.Schedule.RunAllSyncTasks(0.016);
            if (announce)
            {
                SysConsole.Output(OutputType.INIT, "Initially loaded " + c + " chunks...");
            }
        }

        public int Seed;

        public int Seed2;

        public int Seed3;

        public int Seed4;

        public int Seed5;

        public int Seed6;

        void OncePerSecondActions()
        {
            TickClouds();
            List<Location> DelMe = new List<Location>();
            foreach (Chunk chk in LoadedChunks.Values)
            {
                if (chk.LastEdited >= 0)
                {
                    chk.SaveToFile(null);
                }
                bool seen = false;
                foreach (PlayerEntity player in Players)
                {
                    if (player.ShouldLoadChunk(chk.WorldPosition))
                    {
                        seen = true;
                        chk.UnloadTimer = 0;
                        break;
                    }
                }
                if (!seen)
                {
                    chk.UnloadTimer += Delta;
                    if (chk.UnloadTimer > UnloadLimit)
                    {
                        chk.UnloadSafely();
                        DelMe.Add(chk.WorldPosition);
                    }
                }
            }
            foreach (Location loc in DelMe)
            {
                LoadedChunks.Remove(loc);
            }
            if (CFGEdited)
            {
                string cfg = Config.SaveToString();
                TheServer.Schedule.StartASyncTask(() =>
                {
                    Program.Files.WriteText("saves/" + Name + "/region.yml", cfg);
                });
            }
        }

        public double UnloadLimit = 10;

        double opsat;

        public void Tick(double delta)
        {
            Delta = delta;
            GlobalTickTime += Delta;
            if (Delta <= 0)
            {
                return;
            }
            physBoost = 0.1;
            physThisTick = 0;
            opsat += Delta;
            while (opsat > 1.0)
            {
                opsat -= 1.0;
                OncePerSecondActions();
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PhysicsWorld.Update((float)delta);
            sw.Stop();
            TheServer.PhysicsTimeC += sw.Elapsed.TotalMilliseconds;
            TheServer.PhysicsTimes++;
            sw.Reset();
            // TODO: Async tick
            sw.Start();
            for (int i = 0; i < Tickers.Count; i++)
            {
                if (!Tickers[i].Removed && Tickers[i] is PhysicsEntity)
                {
                    ((PhysicsEntity)Tickers[i]).PreTick();
                }
            }
            for (int i = 0; i < Tickers.Count; i++)
            {
                if (!Tickers[i].Removed)
                {
                    Tickers[i].Tick();
                }
            }
            for (int i = 0; i < Tickers.Count; i++)
            {
                if (!Tickers[i].Removed && Tickers[i] is PhysicsEntity)
                {
                    ((PhysicsEntity)Tickers[i]).EndTick();
                }
            }
            for (int i = 0; i < DespawnQuick.Count; i++)
            {
                DespawnEntity(DespawnQuick[i]);
            }
            DespawnQuick.Clear();
            for (int i = 0; i < Joints.Count; i++) // TODO: Optimize!
            {
                if (Joints[i].Enabled && Joints[i] is BaseFJoint)
                {
                    ((BaseFJoint)Joints[i]).Solve();
                }
            }
            sw.Stop();
            TheServer.EntityTimeC += sw.Elapsed.TotalMilliseconds;
            TheServer.EntityTimes++;
        }

        public string Name = null;

        public Server TheServer = null;

        Scheduler LoadTimeScheduler = new Scheduler();

        /// <summary>
        /// Does not return until fully unloaded.
        /// </summary>
        public void UnloadFully()
        {
            // TODO: Transfer all players to another world. Or kick if no worlds available?
            IntHolder counter = new IntHolder(); // TODO: is IntHolder needed here?
            IntHolder total = new IntHolder(); // TODO: is IntHolder needed here?
            List<Chunk> chunks = new List<Chunk>(LoadedChunks.Values);
            foreach (Chunk chunk in chunks)
            {
                total.Value++;
                chunk.UnloadSafely(() => { lock (counter) { counter.Value++; } });
            }
            double z = 0;
            int pval = 0;
            int pvtime = 0;
            while (true)
            {
                z += 0.016;
                if (z > 1.0)
                {
                    lock (counter)
                    {
                        SysConsole.Output(OutputType.INFO, "Got: " + counter.Value + "/" + total.Value + " so far...");
                        if (counter.Value >= total.Value)
                        {
                            break;
                        }
                        if (counter.Value == pval)
                        {
                            pvtime++;
                            if (pvtime > 5)
                            {
                                SysConsole.Output(OutputType.INFO, "Giving up.");
                                return;
                            }
                        }
                        pval = counter.Value;
                    }
                    z = 0;
                }
                Thread.Sleep(16);
                TheServer.Schedule.RunAllSyncTasks(0.016);
            }
            OncePerSecondActions();
            FinalShutdown();
        }

        public void FinalShutdown()
        {
            ChunkManager.Shutdown();
        }

        public void PlaySound(string sound, Location pos, float vol, float pitch)
        {
            bool nan = pos.IsNaN();
            Location cpos = nan ? Location.Zero : ChunkLocFor(pos);
            PlaySoundPacketOut packet = new PlaySoundPacketOut(TheServer, sound, vol, pitch, pos);
            foreach (PlayerEntity player in Players)
            {
                if (nan || player.CanSeeChunk(cpos))
                {
                    player.Network.SendPacket(packet);
                }
            }
        }

        public void PaintBomb(Location pos, byte bcol, float rad = 5f)
        {
            foreach (Location loc in GetBlocksInRadius(pos, 5))
            {
                // TODO: Ray-trace the block?
                BlockInternal bi = GetBlockInternal(loc);
                SetBlockMaterial(loc, (Material)bi.BlockMaterial, bi.BlockData, bcol, (byte)(bi.BlockLocalData | (byte)BlockFlags.EDITED), bi.Damage);
            }
            System.Drawing.Color ccol = Colors.ForByte(bcol);
            ParticleEffectPacketOut pepo = new ParticleEffectPacketOut(ParticleEffectNetType.PAINT_BOMB, rad + 15, pos, new Location(ccol.R / 255f, ccol.G / 255f, ccol.B / 255f));
            foreach (PlayerEntity pe in GetPlayersInRadius(pos, rad + 30)) // TODO: Better particle view dist
            {
                pe.Network.SendPacket(pepo);
            }
            // TODO: Sound effect?
        }

        public void Explode(Location pos, float rad = 5f, bool effect = true, bool breakblock = true, bool applyforce = true, bool doDamage = true)
        {
            float expDamage = 5 * rad;
            CheckThreadValidity();
            if (breakblock)
            {
                int min = (int)Math.Floor(-rad);
                int max = (int)Math.Ceiling(rad);
                for (int x = min; x < max; x++)
                {
                    for (int y = min; y < max; y++)
                    {
                        for (int z = min; z < max; z++)
                        {
                            Location post = new Location(pos.X + x, pos.Y + y, pos.Z + z);
                            // TODO: Defensive wall structuring - trace lines and break as appropriate.
                            if ((post - pos).LengthSquared() <= rad * rad && GetBlockMaterial(post).GetHardness() <= expDamage / (post - pos).Length())
                            {
                                BreakNaturally(post, true);
                            }
                        }
                    }
                }
            }
            if (effect)
            {
                ParticleEffectPacketOut pepo = new ParticleEffectPacketOut(ParticleEffectNetType.EXPLOSION, rad + 15, pos);
                foreach (PlayerEntity pe in GetPlayersInRadius(pos, rad + 30)) // TODO: Better particle view dist
                {
                    pe.Network.SendPacket(pepo);
                }
                // TODO: Sound effect?
            }
            if (applyforce)
            {
                foreach (Entity e in GetEntitiesInRadius(pos, rad * 5)) // TODO: Physent-specific search method?
                {
                    // TODO: Generic entity 'ApplyForce' method
                    if (e is PhysicsEntity)
                    {
                        Location offs = e.GetPosition() - pos;
                        float dpower = (float)((rad * 5) - offs.Length()); // TODO: Efficiency?
                        Location force = new Location(rad, rad, rad * 3) * dpower;
                        ((PhysicsEntity)e).ApplyForce(force);
                    }
                }
            }
            if (doDamage)
            {
                // TODO: DO DAMAGE!
            }
        }
    }
}
