﻿using System;
using Voxalia.Shared;
using System.Diagnostics;
using System.Threading;
using Voxalia.ServerGame.CommandSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.PlayerCommandSystem;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.OtherSystems;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    /// <summary>
    /// The center of all server activity in Shadow Operations.
    /// </summary>
    public partial class Server
    {
        /// <summary>
        /// The primary running server.
        /// </summary>
        public static Server Central = null;

        /// <summary>
        /// Starts up a new server.
        /// </summary>
        public static void Init(string args)
        {
            Central = new Server();
            Central.StartUp();
        }

        public ServerCommands Commands;
        public ServerCVar CVars;

        public PlayerCommandEngine PCEngine;

        public NetworkBase Networking;

        public ItemRegistry Items;

        public ModelEngine Models;

        public AnimationEngine Animations;

        public Scheduler Schedule = new Scheduler();

        bool TickMe = true;

        public void ShutDown()
        {
            SysConsole.Output(OutputType.INFO, "[Shutdown] Starting to close server...");
            foreach (World world in LoadedWorlds)
            {
                while (world.Players.Count > 0)
                {
                    world.Players[0].Kick("Server shutting down.");
                }
                SysConsole.Output(OutputType.INFO, "[Shutdown] Unloading world: " + world.Name);
                world.UnloadFully();
            }
            SysConsole.Output(OutputType.INFO, "[Shutdown] Closing server...");
            ShutDownQuickly();
        }

        public void ShutDownQuickly()
        {
            TickMe = false;
            ConsoleHandler.Close();
        }

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp(Action loaded = null)
        {
            SysConsole.Output(OutputType.INIT, "Launching as new server, this is " + (this == Central ? "" : "NOT ") + "the Central server.");
            SysConsole.Output(OutputType.INIT, "Loading console input handler...");
            ConsoleHandler.Init();
            SysConsole.Output(OutputType.INIT, "Loading command engine...");
            Commands = new ServerCommands();
            Commands.Init(new ServerOutputter(this), this);
            SysConsole.Output(OutputType.INIT, "Loading CVar engine...");
            CVars = new ServerCVar();
            CVars.Init(Commands.Output);
            SysConsole.Output(OutputType.INIT, "Loading default settings...");
            if (Program.Files.Exists("serverdefaultsettings.cfg"))
            {
                string contents = Program.Files.ReadText("serverdefaultsettings.cfg");
                Commands.ExecuteCommands(contents);
            }
            SysConsole.Output(OutputType.INIT, "Loading player command engine...");
            PCEngine = new PlayerCommandEngine();
            SysConsole.Output(OutputType.INIT, "Loading item registry...");
            Items = new ItemRegistry();
            SysConsole.Output(OutputType.INIT, "Loading model handler...");
            Models = new ModelEngine();
            SysConsole.Output(OutputType.INIT, "Loading animation handler...");
            Animations = new AnimationEngine();
            SysConsole.Output(OutputType.INIT, "Building physics world...");
            BuildWorld();
            SysConsole.Output(OutputType.INIT, "Preparing networking...");
            Networking = new NetworkBase(this);
            Networking.Init();
            SysConsole.Output(OutputType.INIT, "Building an initial world...");
            LoadWorld("default");
            if (loaded != null)
            {
                loaded.Invoke();
            }
            SysConsole.Output(OutputType.INIT, "Ticking...");
            // Tick
            double TARGETFPS = 40d;
            Stopwatch Counter = new Stopwatch();
            Stopwatch DeltaCounter = new Stopwatch();
            DeltaCounter.Start();
            double TotalDelta = 0;
            double CurrentDelta = 0d;
            double TargetDelta = 0d;
            int targettime = 0;
            while (TickMe)
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
                TARGETFPS = CVars.g_fps.ValueD;
                if (TARGETFPS < 1 || TARGETFPS > 600)
                {
                    CVars.g_fps.Set("30");
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
                    Tick(TargetDelta);
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
            SysConsole.Output(OutputType.INFO, "[Shutdown] Reached end of server functionality.");
            // TODO: Clean up?
        }

        public PlayerEntity GetPlayerFor(string name)
        {
            string namelow = name.ToLower();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLower() == namelow)
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLower().StartsWith(namelow))
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLower().Contains(namelow))
                {
                    return Players[i];
                }
            }
            return null;
        }
    }
}
