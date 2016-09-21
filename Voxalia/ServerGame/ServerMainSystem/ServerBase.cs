using System;
using System.Collections.Generic;
using System.Text;
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
using FreneticScript;
using Voxalia.Shared.Files;
using FreneticDataSyntax;

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

        public readonly int Port;

        /// <summary>
        /// Serverside file handler.
        /// </summary>
        public FileHandler Files = new FileHandler();

        /// <summary>
        /// Starts up a new server.
        /// </summary>
        public static void Init(string[] args)
        {
            Central = new Server(args.Length > 0 ? Utilities.StringToInt(args[0]) : 28010);
            Central.StartUp(() =>
            {
                if (args.Length > 1)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 1; i < args.Length; i++)
                    {
                        sb.Append(args[i]);
                    }
                    Central.Commands.ExecuteCommands(sb.ToString());
                }
            });
        }

        public Server(int port)
        {
            Files.Init();
            Port = port;
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
        public ItemInfoRegistry ItemInfos;

        /// <summary>
        /// The system that tracks all base items.
        /// </summary>
        public ItemRegistry Items;

        /// <summary>
        /// The system that tracks all item recipes.
        /// </summary>
        public RecipeRegistry Recipes;

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
        /// The system that handles rendering images of blocks.
        /// </summary>
        public BlockImageManager BlockImages;
        
        /// <summary>
        /// Never run unneeded code when in shutdown mode!
        /// </summary>
        public bool ShuttingDown = false;

        Thread CurThread;

        public bool NeedShutdown = false;

        Action shutdownCallback = null;

        /// <summary>
        /// Shuts down the server, saving any necessary data.
        /// </summary>
        public void ShutDown(Action callback = null)
        {
            shutdownCallback = callback;
            if (CurThread != Thread.CurrentThread)
            {
                NeedShutdown = true; // TODO: Lock for this?
                return;
            }
            ShuttingDown = true;
            SysConsole.Output(OutputType.INFO, "[Shutdown] Starting to close server...");
            Schedule.Tasks.Clear();
            foreach (PlayerEntity player in Players)
            {
                player.Kick("Server shutting down.");
            }
            foreach (World world in LoadedWorlds)
            {
                // TODO: Thread-safer shutdown sequence!
                SysConsole.Output(OutputType.INFO, "[Shutdown] Unloading world: " + world.Name);
                world.UnloadFully();
            }
            LoadedWorlds.Clear();
            SysConsole.Output(OutputType.INFO, "[Shutdown] Clearing plugins...");
            Plugins.UnloadPlugins();
            SysConsole.Output(OutputType.INFO, "[Shutdown] Closing server...");
            ShutDownQuickly();
        }

        /// <summary>
        /// Tells the server to shut down, without saving anything.
        /// </summary>
        public void ShutDownQuickly()
        {
            if (CurThread != Thread.CurrentThread)
            {
                CurThread.Abort();
            }
            ShuttingDown = true;
            foreach (World world in LoadedWorlds)
            {
                world.FinalShutdown();
            }
            ConsoleHandler.Close();
            if (shutdownCallback != null)
            {
                shutdownCallback.Invoke();
            }
            if (CurThread == Thread.CurrentThread)
            {
                CurThread.Abort();
            }
        }

        public void CommandInputHandle(object sender, ConsoleCommandEventArgs e)
        {
            Schedule.ScheduleSyncTask(() =>
            {
                Commands.ExecuteCommands(e.Command);
            });
        }

        public PluginManager Plugins;

        public FDSSection Config;

        public List<string> RecentMessages = new List<string>();

        public Object RecentMessagesLock = new Object();

        public void OnConsoleWritten(object sender, ConsoleWrittenEventArgs args)
        {
            lock (RecentMessagesLock)
            {
                RecentMessages.Add(args.Text.Replace("^B", args.BColor));
                if (RecentMessages.Count > 500)
                {
                    RecentMessages.RemoveRange(0, 100);
                }
            }
        }

        public FDSSection GetPlayerConfig(string username)
        {
            if (username.Length == 0)
            {
                return null;
            }
            lock (TickLock)
            {
                PlayerEntity pl = GetPlayerFor(username);
                if (pl != null)
                {
                    return pl.PlayerConfig;
                }
                FDSSection PlayerConfig = null;
                string nl = username.ToLower();
                // TODO: Journaling read
                string fn = "server_player_saves/" + nl[0].ToString() + "/" + nl + ".plr";
                if (Files.Exists(fn))
                {
                    string dat = Files.ReadText(fn);
                    if (dat != null)
                    {
                        PlayerConfig = new FDSSection(dat);
                    }
                }
                return PlayerConfig;
            }
        }

        public Object TickLock = new Object();

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp(Action loaded = null)
        {
            CurThread = Thread.CurrentThread;
            SysConsole.Written += OnConsoleWritten;
            SysConsole.Output(OutputType.INIT, "Launching as new server, this is " + (this == Central ? "" : "NOT ") + "the Central server.");
            SysConsole.Output(OutputType.INIT, "Loading console input handler...");
            ConsoleHandler.Init();
            ConsoleHandler.OnCommandInput += CommandInputHandle;
            SysConsole.Output(OutputType.INIT, "Loading command engine...");
            Commands = new ServerCommands();
            Commands.Init(new ServerOutputter(this), this);
            SysConsole.Output(OutputType.INIT, "Loading CVar engine...");
            CVars = new ServerCVar();
            CVars.Init(this, Commands.Output);
            SysConsole.Output(OutputType.INIT, "Loading default settings...");
            Config = new FDSSection(Files.ReadText("server_config.fds"));
            if (Files.Exists("serverdefaultsettings.cfg"))
            {
                string contents = Files.ReadText("serverdefaultsettings.cfg");
                Commands.ExecuteCommands(contents);
            }
            SysConsole.Output(OutputType.INIT, "Loading player command engine...");
            PCEngine = new PlayerCommandEngine();
            SysConsole.Output(OutputType.INIT, "Loading item registry...");
            ItemInfos = new ItemInfoRegistry();
            Items = new ItemRegistry(this);
            Recipes = new RecipeRegistry() { TheServer = this };
            SysConsole.Output(OutputType.INIT, "Loading model handler...");
            Models = new ModelEngine(this);
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
            foreach (string str in Config.GetStringList("server.worlds") ?? new List<string>())
            {
                LoadWorld(str.ToLowerFast());
            }
            SysConsole.Output(OutputType.INIT, "Preparing block image system...");
            BlockImages = new BlockImageManager();
            BlockImages.Init(this);
            if (loaded != null)
            {
                loaded.Invoke();
            }
            SysConsole.Output(OutputType.INIT, "Ticking...");
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
                        if (NeedShutdown)
                        {
                            CurThread = Thread.CurrentThread;
                            ShutDown(shutdownCallback);
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

        public void AutorunScripts()
        {
            string[] files = System.IO.Directory.GetFiles(Environment.CurrentDirectory + "/data/scripts/server/", "*.cfg", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string cmd = System.IO.File.ReadAllText(file).Replace("\r", "").Replace("\0", "\\0");
                Commands.CommandSystem.PrecalcScript(file.Replace(Environment.CurrentDirectory, ""), cmd);
            }
            Commands.CommandSystem.RunPrecalculated();
        }

        /// <summary>
        /// Gets the player entity associated with a given playername.
        /// </summary>
        public PlayerEntity GetPlayerFor(string name)
        {
            string namelow = name.ToLowerFast();
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerFast() == namelow)
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerFast().StartsWith(namelow))
                {
                    return Players[i];
                }
            }
            for (int i = 0; i < Players.Count; i++)
            {
                if (Players[i].Name.ToLowerFast().Contains(namelow))
                {
                    return Players[i];
                }
            }
            return null;
        }
    }
}
