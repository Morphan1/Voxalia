using System;
using System.Collections.Generic;
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

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        /// <summary>
        /// How much time has elapsed since the last tick started.
        /// </summary>
        public double Delta;

        public double GlobalTickTime = 0;

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
            string fname = "saves/" + Name + "/region.yml";
            if (Program.Files.Exists(fname))
            {
                Config = new YAMLConfiguration(Program.Files.ReadText(fname));
            }
            else
            {
                Config = new YAMLConfiguration("");
            }
            Config.Changed += new EventHandler(configChanged);
            Config.Set("general.IMPORTANT_NOTE", "Edit this configuration at your own risk!");
            Config.Set("general.name", Name);
            Config.Default("general.seed", Utilities.UtilRandom.Next(SeedMax) - SeedMax / 2);
            CFGEdited = true;
            Seed = Config.ReadInt("general.seed", 100);
            Random seedGen = new Random(Seed);
            Seed2 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed3 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed4 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed5 = (seedGen.Next(SeedMax) - SeedMax / 2);
            EntityConstructors.Add(EntityType.ITEM, new ItemEntityConstructor());
            EntityConstructors.Add(EntityType.BLOCK_ITEM, new BlockItemEntityConstructor());
            EntityConstructors.Add(EntityType.GLOWSTICK, new GlowstickEntityConstructor());
            EntityConstructors.Add(EntityType.MODEL, new ModelEntityConstructor());
            EntityConstructors.Add(EntityType.SMOKE_GRENADE, new SmokegrenadeEntityConstructor());
            LoadRegion(new Location(-MaxViewRadiusInChunks * 30), new Location(MaxViewRadiusInChunks * 30), true);
            TheServer.Schedule.RunAllSyncTasks(0.016); // TODO: Separate per-world scheduler // Also don't freeze the entire server just because we're waiting on chunks >.>
            SysConsole.Output(OutputType.INIT, "Finished building chunks! Now have " + LoadedChunks.Count + " chunks!");
        }

        const int SeedMax = ushort.MaxValue;

        bool CFGEdited = false;

        void configChanged(object sender, EventArgs e)
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
            foreach (Chunk o in LoadedChunks.Values)
            {
                if (o.LastEdited >= 0)
                {
                    Chunk to = o;
                    Action cb = null;/*
                    if (ChunksToDestroy.Contains(o))
                    {
                        cb = () =>
                        {
                            TheServer.Schedule.ScheduleSyncTask(() =>
                            {
                                if (ChunksToDestroy.Contains(to) && to.LastEdited < 0) // TODO: Handle rare case
                                {
                                    ChunksToDestroy.Remove(to);
                                    LoadedChunks.Remove(to.WorldPosition);
                                }
                            });
                        };
                    }*/
                    o.SaveToFile(cb);
                }
                // TODO: If distant from all players, unload
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

        double opsat;

        public void Tick(double delta)
        {
            CheckThreadValidity();
            Delta = delta;
            GlobalTickTime += Delta;
            if (Delta <= 0)
            {
                return;
            }
            opsat += Delta;
            while (opsat > 1.0)
            {
                opsat -= 1.0;
                OncePerSecondActions();
            }
            PhysicsWorld.Update((float)delta); // TODO: More specific settings?
            // TODO: Async tick
            for (int i = 0; i < Tickers.Count; i++)
            {
                Tickers[i].Tick();
            }
            for (int i = 0; i < Tickers.Count; i++)
            {
                if (Tickers[i] is PhysicsEntity)
                {
                    ((PhysicsEntity)Tickers[i]).EndTick();
                }
            }
            for (int i = 0; i < Joints.Count; i++) // TODO: Optimize!
            {
                if (Joints[i].Enabled && Joints[i] is BaseFJoint)
                {
                    ((BaseFJoint)Joints[i]).Solve();
                }
            }
        }

        public string Name = null;

        public Server TheServer = null;

        Scheduler LoadTimeScheduler = new Scheduler();

        /// <summary>
        /// Does not return until fully unloaded.
        /// </summary>
        public void UnloadFully()
        {
            CheckThreadValidity();
            // TODO: Transfer all players to another world.
            IntHolder counter = new IntHolder(); // TODO: is IntHolder needed here?
            IntHolder total = new IntHolder(); // TODO: is IntHolder needed here?
            foreach (Chunk chunk in LoadedChunks.Values)
            {
                total.Value++;
                chunk.UnloadSafely(() => { counter.Value++; });
            }
            while (counter.Value < total.Value)
            {
                Thread.Sleep(16);
            }
            OncePerSecondActions();
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
                                BreakNaturally(post, true, 0);
                            }
                        }
                    }
                }
            }
            if (effect)
            {
                ParticleEffectPacketOut pepo = new ParticleEffectPacketOut(ParticleEffectNetType.EXPLOSION, rad + 30, pos);
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
