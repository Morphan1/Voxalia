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

namespace Voxalia.ServerGame.WorldSystem
{
    public class World
    {
        public FDSSection Config;

        public string Name;

        public Dictionary<Vector2i, Region> LoadedRegions = new Dictionary<Vector2i, Region>();

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
            CFGEdited = true;
            Seed = Config.GetInt("general.seed", DefaultSeed).Value;
            SpawnPoint = Location.FromString(Config.GetString("general.spawnpoint", DefaultSpawnPoint));
            Random seedGen = new Random(Seed);// TODO: Own random method that doesn't depend on C# impl!
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

        public void Start()
        {
            if (Execution != null)
            {
                return;
            }
            Execution = new Thread(new ThreadStart(MainThread));
            Execution.Start();
        }

        public void LoadRegion(int x, int y)
        {
            Region rg = new Region();
            rg.TheServer = TheServer;
            rg.TheWorld = this;
            Vector2i pos = new Vector2i(x, y);
            rg.Position = pos;
            rg.BuildWorld();
            LoadedRegions.Add(pos, rg);
        }
        
        private void MainThread()
        {
            LoadConfig();
            LoadRegion(0, 0);
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
                            UnloadFully();
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

        public void UnloadFully()
        {
            NeedShutdown = true;
            if (Thread.CurrentThread != Execution)
            {
                return;
            }
            // TODO: Lock safely!
            foreach (Region reg in LoadedRegions.Values)
            {
                reg.UnloadFully();
            }
            LoadedRegions.Clear();
            Execution.Abort();
            Execution = null;
        }

        bool NeedShutdown = false;
        
        public void FinalShutdown()
        {
            foreach (Region reg in LoadedRegions.Values)
            {
                reg.FinalShutdown();
            }
        }

        Object SaveWorldCFGLock = new Object();

        public void OncePerSecondActions()
        {
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

        public void Tick(double delta)
        {
            Schedule.RunAllSyncTasks(delta);
            foreach (Region r in LoadedRegions.Values)
            {
                r.Tick(delta);
            }
        }
    }
}
