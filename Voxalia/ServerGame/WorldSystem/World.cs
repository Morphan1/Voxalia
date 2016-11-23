//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared.Collision;
using System.Threading;
using FreneticDataSyntax;
using Voxalia.Shared;
using System.Diagnostics;
using FreneticScript;

namespace Voxalia.ServerGame.WorldSystem
{
    public class World
    {
        public FDSSection Config;

        public string Name;

        /// <summary>
        /// Represents the only region currently contained by a world.
        /// May in the future by changed to a dictionary of separate worldly regions.
        /// </summary>
        public Region MainRegion = null;

        public Server TheServer;

        public Thread Execution = null;

        public const int DefaultSeed = 100;

        public const string DefaultSpawnPoint = "0,0,50";

        const int SeedMax = ushort.MaxValue;

        public void LoadConfig()
        {
            string folder = "saves/" + Name;
            TheServer.Files.CreateDirectory(folder);
            // TODO: Journaling read
            string fname = folder + "/world.fds";
            if (TheServer.Files.Exists(fname))
            {
                Config = new FDSSection(TheServer.Files.ReadText(fname));
            }
            else
            {
                Config = new FDSSection();
            }
            Config.Set("general.IMPORTANT_NOTE", "Edit this configuration at your own risk!");
            Config.Set("general.name", Name);
            Config.Default("general.seed", Utilities.UtilRandom.Next(SeedMax) - SeedMax / 2);
            Config.Default("general.spawnpoint", new Location(0, 0, 50).ToString());
            Config.Default("general.flat", "false");
            CFGEdited = true;
            Seed = Config.GetInt("general.seed", DefaultSeed).Value;
            SpawnPoint = Location.FromString(Config.GetString("general.spawnpoint", DefaultSpawnPoint));
            Flat = Config.GetString("general.flat", "false").ToString().ToLowerFast() == "true";
            MTRandom seedGen = new MTRandom(39, (ulong)Seed);
            Seed2 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed3 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed4 = (seedGen.Next(SeedMax) - SeedMax / 2);
            Seed5 = (seedGen.Next(SeedMax) - SeedMax / 2);
        }

        public bool CFGEdited;

        public Location SpawnPoint;
        
        public int Seed;

        public int Seed2;

        public int Seed3;

        public int Seed4;

        public int Seed5;

        public Scheduler Schedule = new Scheduler();

        public bool Flat = false;

        public void Start()
        {
            if (Execution != null)
            {
                return;
            }
            Execution = new Thread(new ThreadStart(MainThread));
            Execution.Start();
        }

        public void LoadRegion()
        {
            if (MainRegion != null)
            {
                return;
            }
            Region rg = new Region();
            rg.TheServer = TheServer;
            rg.TheWorld = this;
            rg.BuildWorld();
            MainRegion = rg;
        }
        
        private void MainThread()
        {
            LoadConfig();
            LoadRegion();
            // Tick
            double TARGETFPS = 30d;
            Stopwatch Counter = new Stopwatch();
            Stopwatch DeltaCounter = new Stopwatch();
            DeltaCounter.Start();
            double TotalDelta = 0;
            double CurrentDelta = 0d;
            double TargetDelta = 0d;
            int targettime = 0;
            try
            {
                while (true)
                {
                    // Update the tick time usage counter
                    Counter.Reset();
                    Counter.Start();
                    // Update the tick delta counter
                    DeltaCounter.Stop();
                    // Delta time = Elapsed ticks * (ticks/second)
                    CurrentDelta = ((double)DeltaCounter.ElapsedTicks) / ((double)Stopwatch.Frequency);
                    // Begin the delta counter to find out how much time is /really/ slept+ticked for
                    DeltaCounter.Reset();
                    DeltaCounter.Start();
                    // How much time should pass between each tick ideally
                    TARGETFPS = TheServer.CVars.g_fps.ValueD;
                    if (TARGETFPS < 1 || TARGETFPS > 600)
                    {
                        TheServer.CVars.g_fps.Set("30");
                        TARGETFPS = 30;
                    }
                    TargetDelta = (1d / TARGETFPS);
                    // How much delta has been built up
                    TotalDelta += CurrentDelta;
                    while (TotalDelta > TargetDelta * 3)
                    {
                        // Lagging - cheat to catch up!
                        TargetDelta *= 2;
                    }
                    // As long as there's more delta built up than delta wanted, tick
                    while (TotalDelta > TargetDelta)
                    {
                        if (NeedShutdown)
                        {
                            UnloadFully(null);
                            return;
                        }
                        lock (TickLock)
                        {
                            Tick(TargetDelta);
                        }
                        TotalDelta -= TargetDelta;
                    }
                    // The tick is done, stop measuring it
                    Counter.Stop();
                    // Only sleep for target milliseconds/tick minus how long the tick took... this is imprecise but that's okay
                    targettime = (int)((1000d / TARGETFPS) - Counter.ElapsedMilliseconds);
                    // Only sleep at all if we're not lagging
                    if (targettime > 0)
                    {
                        // Try to sleep for the target time - very imprecise, thus we deal with precision inside the tick code
                        Thread.Sleep(targettime);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
        }

        public Object TickLock = new Object();

        Action UnloadCallback = null;

        public void UnloadFully(Action wrapUp)
        {
            if (wrapUp != null)
            {
                UnloadCallback = wrapUp;
            }
            NeedShutdown = true;
            if (Thread.CurrentThread != Execution)
            {
                return;
            }
            // TODO: Lock safely!
            MainRegion.UnloadFully();
            MainRegion = null;
            UnloadCallback?.Invoke();
            Execution.Abort();
            Execution = null;
        }

        bool NeedShutdown = false;
        
        public void FinalShutdown()
        {
            MainRegion.FinalShutdown();
        }

        Object SaveWorldCFGLock = new Object();

        long previous_eid = 0;

        public void OncePerSecondActions()
        {
            long cid;
            lock (TheServer.CIDLock)
            {
                cid = TheServer.cID;
            }
            if (cid != previous_eid)
            {
                previous_eid = cid;
                Schedule.StartASyncTask(() =>
                {
                    TheServer.Files.WriteText("saves/" + Name + "/eid.txt", cid.ToString());
                });
            }
            if (CFGEdited)
            {
                string cfg = Config.SaveToString();
                Schedule.StartASyncTask(() =>
                {
                    // TODO: Journaling save.
                    lock (SaveWorldCFGLock)
                    {
                        TheServer.Files.WriteText("saves/" + Name + "/world.fds", cfg);
                    }
                });
            }
        }

        double ops = 0;

        public void Tick(double delta)
        {
            ops += delta;
            if (ops > 1)
            {
                ops = 0;
                OncePerSecondActions();
            }
            Schedule.RunAllSyncTasks(delta);
            MainRegion.Tick(delta);
        }
    }
}
