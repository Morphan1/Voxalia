//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
using FreneticDataSyntax;

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

        public void ChunkSendToAll(AbstractPacketOut packet, Vector3i cpos)
        {
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].CanSeeChunk(cpos))
                {
                    Players[i].Network.SendPacket(packet);
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
                Players[i].SendMessage(TextChannel.BROADCAST, message);
            }
        }

        public int MaxViewRadiusInChunks = 4;

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
            PhysicsWorld.TimeStepSettings.TimeStepDuration = 1f / TheServer.CVars.g_fps.ValueF;
            Collision = new CollisionUtil(PhysicsWorld);
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
        
        void OncePerSecondActions()
        {
            TickClouds();
            List<Vector3i> DelMe = new List<Vector3i>();
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
            foreach (Vector3i loc in DelMe)
            {
                LoadedChunks.Remove(loc);
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
            physBoost = defPhysBoost;
            physThisTick = 0;
            opsat += Delta;
            while (opsat > 1.0)
            {
                opsat -= 1.0;
                OncePerSecondActions();
            }
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PhysicsWorld.Update((double)delta);
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
        
        public Server TheServer = null;

        public World TheWorld = null;
        
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
                            if (pvtime > 15)
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
                TheWorld.Schedule.RunAllSyncTasks(0.016);
            }
            OncePerSecondActions();
            FinalShutdown();
        }

        public void FinalShutdown()
        {
            ChunkManager.Shutdown();
        }

        public void PlaySound(string sound, Location pos, double vol, double pitch)
        {
            bool nan = pos.IsNaN();
            Vector3i cpos = nan ? Vector3i.Zero : ChunkLocFor(pos);
            PlaySoundPacketOut packet = new PlaySoundPacketOut(TheServer, sound, vol, pitch, pos);
            foreach (PlayerEntity player in Players)
            {
                if (nan || player.CanSeeChunk(cpos))
                {
                    player.Network.SendPacket(packet);
                }
            }
        }

        public void PaintBomb(Location pos, byte bcol, double rad = 5f)
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

        public void Explode(Location pos, double rad = 5f, bool effect = true, bool breakblock = true, bool applyforce = true, bool doDamage = true)
        {
            if (doDamage)
            {
                // TODO: DO DAMAGE!
            }
            double expDamage = 5 * rad;
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
                ParticleEffectPacketOut pepo = new ParticleEffectPacketOut(ParticleEffectNetType.EXPLOSION, rad, pos);
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
                        double dpower = (double)((rad * 5) - offs.Length()); // TODO: Efficiency?
                        Location force = new Location(1, 1, 3) * dpower;
                        ((PhysicsEntity)e).ApplyForce(force);
                    }
                }
            }
        }
    }
}
