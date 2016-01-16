using System;
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
using Voxalia.ServerGame.PluginSystem;
using FreneticScript.CommandSystem;

namespace Voxalia.ServerGame.ServerMainSystem
{
    /// <summary>
    /// The center of all server activity in Voxalia.
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

        /// <summary>
        /// The system that handles all command line input from the server.
        /// </summary>
        public ServerCommands Commands;

        /// <summary>
        /// The system that handles all variables the server admin can control.
        /// </summary>
        public ServerCVar CVars;

        /// <summary>
        /// The system that handles all command line input from players on the server.
        /// </summary>
        public PlayerCommandEngine PCEngine;

        /// <summary>
        /// The system that handles all server-to-client networking.
        /// </summary>
        public NetworkBase Networking;

        /// <summary>
        /// The system that tracks all 'item types' for use by ItemStacks.
        /// </summary>
        public ItemInfoRegistry Items;

        /// <summary>
        /// The system that tracks all 3D models, for collision tracing purposes.
        /// </summary>
        public ModelEngine Models;

        /// <summary>
        /// The system that handles 3D model animation, for collision tracing and positioning purposes.
        /// </summary>
        public AnimationEngine Animations;

        /// <summary>
        /// The scheduling engine used for general server tasks.
        /// </summary>
        public Scheduler Schedule = new Scheduler();

        /// <summary>
        /// Whether the server needs to keep ticking - setting this false will end the server central tick process soon.
        /// </summary>
        bool TickMe = true;

        /// <summary>
        /// Shuts down the server, saving any necessary data.
        /// </summary>
        public void ShutDown()
        {
            SysConsole.Output(OutputType.INFO, "[Shutdown] Starting to close server...");
            foreach (Region region in LoadedRegions)
            {
                while (region.Players.Count > 0)
                {
                    region.Players[0].Kick("Server shutting down.");
                }
                SysConsole.Output(OutputType.INFO, "[Shutdown] Unloading world: " + region.Name);
                region.UnloadFully();
            }
            SysConsole.Output(OutputType.INFO, "[Shutdown] Closing server...");
            ShutDownQuickly();
        }

        /// <summary>
        /// Tells the server to shut down, without saving anything.
        /// </summary>
        public void ShutDownQuickly()
        {
            TickMe = false;
            ConsoleHandler.Close();
        }

        public void CommandInputHandle(object sender, ConsoleCommandEventArgs e)
        {
            Schedule.ScheduleSyncTask(() =>
            {
                Commands.ExecuteCommands(e.Command);
            });
        }

        public PluginManager Plugins;

        public YAMLConfiguration Config;

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp(Action loaded = null)
        {
            SysConsole.Output(OutputType.INIT, "Launching as new server, this is " + (this == Central ? "" : "NOT ") + "the Central server.");
            SysConsole.Output(OutputType.INIT, "Loading console input handler...");
            ConsoleHandler.Init();
            ConsoleHandler.OnCommandInput += CommandInputHandle;
            SysConsole.Output(OutputType.INIT, "Loading command engine...");
            Commands = new ServerCommands();
            Commands.Init(new ServerOutputter(this), this);
            SysConsole.Output(OutputType.INIT, "Loading CVar engine...");
            CVars = new ServerCVar();
            CVars.Init(Commands.Output);
            SysConsole.Output(OutputType.INIT, "Loading default settings...");
            Config = new YAMLConfiguration(Program.Files.ReadText("server_config.yml"));
            if (Program.Files.Exists("serverdefaultsettings.cfg"))
            {
                string contents = Program.Files.ReadText("serverdefaultsettings.cfg");
                Commands.ExecuteCommands(contents);
            }
            SysConsole.Output(OutputType.INIT, "Loading player command engine...");
            PCEngine = new PlayerCommandEngine();
            SysConsole.Output(OutputType.INIT, "Loading item registry...");
            Items = new ItemInfoRegistry();
            SysConsole.Output(OutputType.INIT, "Loading model handler...");
            Models = new ModelEngine();
            SysConsole.Output(OutputType.INIT, "Loading animation handler...");
            Animations = new AnimationEngine();
            SysConsole.Output(OutputType.INIT, "Preparing networking...");
            Networking = new NetworkBase(this);
            Networking.Init();
            SysConsole.Output(OutputType.INIT, "Loading plugins...");
            Plugins = new PluginManager(this);
            Plugins.Init();
            SysConsole.Output(OutputType.INIT, "Loading scripts...");
            AutorunScripts();
            SysConsole.Output(OutputType.INIT, "Building initial world(s)...");
            foreach (string str in Config.ReadStringList("server.regions"))
            {
                LoadRegion(str.ToLowerInvariant());
            }
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
            SysConsole.Output(OutputType.INFO, "[Shutdown] Clearing plugins...");
            Plugins.UnloadPlugins();
            SysConsole.Output(OutputType.INFO, "[Shutdown] Exiting server main!");
        }

        public void AutorunScripts()
        {
            string[] files = System.IO.Directory.GetFiles(Environment.CurrentDirectory + "/data/scripts/server/", "*.cfg", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                try
                {
                    string cmd = System.IO.File.ReadAllText(file).Replace("\r", "").Replace("\0", "\\0");
                    if (cmd.StartsWith("/// AUTORUN\n"))
                    {
                        CommandScript.SeparateCommands(file.Replace(Environment.CurrentDirectory, ""), cmd, Commands.CommandSystem).ToQueue(Commands.CommandSystem).Execute();
                    }
                }
                catch (Exception ex)
                {
                    SysConsole.Output("Handling autorun script '" + file.Replace(Environment.CurrentDirectory, "") + "'", ex);
                }
            }
        }

        /// <summary>
        /// Gets the player entity associated with a given playername.
        /// </summary>
        public PlayerEntity GetPlayerFor(string name)
        {
            string namelow = name.ToLowerInvariant();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerInvariant() == namelow)
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerInvariant().StartsWith(namelow))
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerInvariant().Contains(namelow))
                {
                    return Players[i];
                }
            }
            return null;
        }
    }
}
