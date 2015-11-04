using System;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.UISystem;
using OpenTK.Input;
using Voxalia.ClientGame.CommandSystem;
using System.Diagnostics;
using Voxalia.ClientGame.NetworkSystem;
using Voxalia.ClientGame.AudioSystem;
using Voxalia.ClientGame.GraphicsSystems.ParticleSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ServerGame.ServerMainSystem;
using System.Threading;
using System.Drawing;

namespace Voxalia.ClientGame.ClientMainSystem
{
    /// <summary>
    /// The center of all client activity in Shadow Operations.
    /// </summary>
    public partial class Client
    {
        /// <summary>
        /// The linked server if playing singleplayer.
        /// </summary>
        public Server LocalServer = null;

        /// <summary>
        /// The scheduling engine used for general client tasks.
        /// </summary>
        public Scheduler Schedule = new Scheduler();

        /// <summary>
        /// The primary running client.
        /// </summary>
        public static Client Central = null;

        /// <summary>
        /// Starts up a new server.
        /// </summary>
        public static void Init(string args)
        {
            Central = new Client();
            Central.StartUp(args);
        }

        /// <summary>
        /// The main Window used by the game.
        /// </summary>
        public GameWindow Window;

        /// <summary>
        /// Handles all command line (console) input from the client via Frenetic.
        /// </summary>
        public ClientCommands Commands;

        /// <summary>
        /// Handles all client-editable variables.
        /// </summary>
        public ClientCVar CVars;

        /// <summary>
        /// Handles client-to-server networking.
        /// </summary>
        public NetworkBase Network;

        /// <summary>
        /// The username the player will connect under.
        /// </summary>
        public string Username = "Player" + new Random().Next(1000);

        /// <summary>
        /// The texture of an Item Frame, for UI rendering.
        /// </summary>
        public Texture ItemFrame;

        /// <summary>
        /// The current 'Screen' object.
        /// </summary>
        public Screen CScreen;

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp(string args)
        {
            SysConsole.Output(OutputType.INIT, "Launching as new client, this is " + (this == Central ? "" : "NOT ") + "the Central client.");
            SysConsole.Output(OutputType.INIT, "Loading command system...");
            Commands = new ClientCommands();
            Commands.Init(new ClientOutputter(this), this);
            SysConsole.Output(OutputType.INIT, "Loading CVar system...");
            CVars = new ClientCVar();
            CVars.Init(Commands.Output);
            SysConsole.Output(OutputType.INIT, "Loading default settings...");
            if (Program.Files.Exists("clientdefaultsettings.cfg"))
            {
                string contents = Program.Files.ReadText("clientdefaultsettings.cfg");
                Commands.ExecuteCommands(contents);
            }
            Commands.ExecuteCommands(args);
            SysConsole.Output(OutputType.INIT, "Generating window...");
            Window = new GameWindow(CVars.r_width.ValueI, CVars.r_height.ValueI, new GraphicsMode(24, 24, 0, 0), Program.GameName + " v" + Program.GameVersion,
                GameWindowFlags.Default, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            Window.WindowState = CVars.r_fullscreen.ValueB ? WindowState.Fullscreen : WindowState.Normal;
            Window.WindowBorder = WindowBorder.Fixed;
            Window.Load += new EventHandler<EventArgs>(Window_Load);
            Window.RenderFrame += new EventHandler<FrameEventArgs>(Window_RenderFrame);
            Window.Mouse.Move += new EventHandler<MouseMoveEventArgs>(MouseHandler.Window_MouseMove);
            Window.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyHandler.PrimaryGameWindow_KeyDown);
            Window.KeyPress += new EventHandler<KeyPressEventArgs>(KeyHandler.PrimaryGameWindow_KeyPress);
            Window.KeyUp += new EventHandler<KeyboardKeyEventArgs>(KeyHandler.PrimaryGameWindow_KeyUp);
            Window.Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(KeyHandler.Mouse_Wheel);
            Window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(KeyHandler.Mouse_ButtonDown);
            Window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(KeyHandler.Mouse_ButtonUp);
            Window.Closed += Window_Closed;
            onVsyncChanged(CVars.r_vsync, null);
            Window.Run(1, CVars.r_maxfps.ValueD);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // TODO: Cleanup!
        }

        /// <summary>
        /// The system that manages textures (images) on the client.
        /// </summary>
        public TextureEngine Textures;

        /// <summary>
        /// The system that manages OpenGL shaders on the client.
        /// </summary>
        public ShaderEngine Shaders;

        /// <summary>
        /// The system that manages internal render-ready fonts on the client.
        /// </summary>
        public GLFontEngine Fonts;

        /// <summary>
        /// The system that manages text rendering systems ("Font Sets") on the client.
        /// </summary>
        public FontSetEngine FontSets;
        
        /// <summary>
        /// The system that manages misc. rendering tasks for the client.
        /// </summary>
        public Renderer Rendering;
        
        /// <summary>
        /// The system that manages 3D models on the client.
        /// </summary>
        public ModelEngine Models;

        /// <summary>
        /// The system that manages 3D model animation sets on the client.
        /// </summary>
        public AnimationEngine Animations;

        /// <summary>
        /// The system that manages sounds (audio) on the client.
        /// </summary>
        public SoundEngine Sounds;

        /// <summary>
        /// The system that manages particle effects (quick simple 3D-rendered-only objects) for the client.
        /// </summary>
        public ParticleHelper Particles;

        /// <summary>
        /// The system that manages block textures on the client.
        /// </summary>
        public TextureBlock TBlock;

        /// <summary>
        /// The current PlayerEntity in the game.
        /// Can be null if not in a game.
        /// </summary>
        public PlayerEntity Player;

        /// <summary>
        /// OpenGL's "vendor" string.
        /// </summary>
        public string GLVendor;

        /// <summary>
        /// OpenGL's "version" string.
        /// </summary>
        public string GLVersion;

        /// <summary>
        /// OpenGL's "renderer" string.
        /// </summary>
        public string GLRenderer;

        /// <summary>
        /// Called when the window is loading, only to be used by the startup process.
        /// </summary>
        void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "Window generated!");
            SysConsole.Output(OutputType.INIT, "Loading textures...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem();
            ItemFrame = Textures.GetTexture("ui/hud/item_frame");
            TBlock = new TextureBlock();
            TBlock.Generate(this, CVars, Textures);
            SysConsole.Output(OutputType.INIT, "Loading shaders...");
            Shaders = new ShaderEngine();
            GLVendor = GL.GetString(StringName.Vendor);
            CVars.s_glvendor.Value = GLVendor;
            GLVersion = GL.GetString(StringName.Version);
            CVars.s_glversion.Value = GLVersion;
            GLRenderer = GL.GetString(StringName.Renderer);
            CVars.s_glrenderer.Value = GLRenderer;
            if (GLVendor.ToLower().Contains("intel"))
            {
                SysConsole.Output(OutputType.INIT, "Disabling good graphics (Appears to be Intel: '" + GLVendor + "')");
                Shaders.MCM_GOOD_GRAPHICS = false;
            }
            Shaders.InitShaderSystem();
            SysConsole.Output(OutputType.INIT, "Loading fonts...");
            Fonts = new GLFontEngine(Shaders);
            Fonts.Init();
            FontSets = new FontSetEngine(Fonts);
            FontSets.Init();
            SysConsole.Output(OutputType.INIT, "Loading animation engine...");
            Animations = new AnimationEngine();
            SysConsole.Output(OutputType.INIT, "Loading model engine...");
            Models = new ModelEngine();
            Models.Init(Animations);
            SysConsole.Output(OutputType.INIT, "Loading rendering helper...");
            Rendering = new Renderer(Textures, Shaders);
            Rendering.Init();
            SysConsole.Output(OutputType.INIT, "Loading general graphics settings...");
            CVars.r_vsync.OnChanged += onVsyncChanged;
            onVsyncChanged(CVars.r_vsync, null);
            CVars.r_godray_color.OnChanged += onGodrayColorChanged;
            onGodrayColorChanged(CVars.r_godray_color, null);
            SysConsole.Output(OutputType.INIT, "Loading UI Console...");
            UIConsole.InitConsole();
            SysConsole.Output(OutputType.INIT, "Preparing rendering engine...");
            InitRendering();
            SysConsole.Output(OutputType.INIT, "Loading particle effect engine...");
            Particles = new ParticleHelper(this) { Engine = new ParticleEngine(this) };
            SysConsole.Output(OutputType.INIT, "Preparing mouse and keyboard handlers...");
            KeyHandler.Init();
            SysConsole.Output(OutputType.INIT, "Building the sound system...");
            Sounds = new SoundEngine();
            Sounds.Init(CVars);
            SysConsole.Output(OutputType.INIT, "Building game world...");
            BuildWorld();
            SysConsole.Output(OutputType.INIT, "Preparing networking...");
            Network = new NetworkBase(this);
            SysConsole.Output(OutputType.INIT, "Playing background music...");
            BackgroundMusic();
            CVars.a_musicvolume.OnChanged += onMusicVolumeChanged;
            CVars.a_musicpitch.OnChanged += onMusicPitchChanged;
            CVars.a_music.OnChanged += onMusicChanged;
            SysConsole.Output(OutputType.INIT, "Setting up screens...");
            TheMainMenuScreen = new MainMenuScreen() { TheClient = this };
            TheGameScreen = new GameScreen() { TheClient = this };
            TheChunkWaitingScreen = new ChunkWaitingScreen() { TheClient = this };
            TheSingleplayerMenuScreen = new SingleplayerMenuScreen() { TheClient = this };
            TheInventoryScreen = new InventoryScreen() { TheClient = this };
            TheEquipmentScreen = new EquipmentScreen() { TheClient = this };
            TheMainMenuScreen.Init();
            TheGameScreen.Init();
            TheChunkWaitingScreen.Init();
            TheSingleplayerMenuScreen.Init();
            TheInventoryScreen.Init();
            TheEquipmentScreen.Init();
            ShowMainMenu();
        }

        public void onGodrayColorChanged(object obj, EventArgs e)
        {
            godrayCol = Location.FromString(CVars.r_godray_color.Value);
        }

        /// <summary>
        /// Called or call when the value of the VSync CVar changes, or VSync needs to be recalculated.
        /// </summary>
        /// <param name="obj">.</param>
        /// <param name="e">.</param>
        public void onVsyncChanged(object obj, EventArgs e)
        {
            Window.VSync = CVars.r_vsync.ValueB ? VSyncMode.Adaptive : VSyncMode.Off;
        }
        
        /// <summary>
        /// Shows the 'game' screen to the client - delays until chunks are loaded.
        /// </summary>
        public void ShowGame()
        {
            if (IsWaitingOnChunks())
            {
                TheChunkWaitingScreen.LACS = new LoadAllChunksSystem(TheRegion);
                TheChunkWaitingScreen.LACS.LoadAll(() =>
                {
                    Schedule.ScheduleSyncTask(() =>
                    {
                        // TODO: handle cancel-button cancelling neatly
                        CScreen = TheGameScreen;
                        CScreen.SwitchTo();
                        TheChunkWaitingScreen.LACS = null;
                    });
                });
            }
            else
            {
                CScreen = TheGameScreen;
                CScreen.SwitchTo();
            }
        }

        /// <summary>
        /// Shows the 'equipment' in-game menu screen.
        /// </summary>
        public void ShowEquipment()
        {
            CScreen = TheEquipmentScreen;
            CScreen.SwitchTo();
        }

        /// <summary>
        /// Shows the 'inventory' in-game menu screen.
        /// </summary>
        public void ShowInventory()
        {
            CScreen = TheInventoryScreen;
            CScreen.SwitchTo();
        }

        /// <summary>
        /// Shows the 'singleplayer' main menu screen to the client.
        /// </summary>
        public void ShowSingleplayer()
        {
            CScreen = TheSingleplayerMenuScreen;
            CScreen.SwitchTo();
        }

        /// <summary>
        /// Shows the main menu screen to the client.
        /// </summary>
        public void ShowMainMenu()
        {
            CScreen = TheMainMenuScreen;
            CScreen.SwitchTo();
        }

        /// <summary>
        /// Shows the 'waiting on chunks' menu screen to the client.
        /// </summary>
        public void ShowChunkWaiting()
        {
            CScreen = TheChunkWaitingScreen;
            CScreen.SwitchTo();
        }

        /// <summary>
        /// For use by ProcessChunks() alone.
        /// </summary>
        byte pMode = 0;

        /// <summary>
        /// Loads all unloaded but waiting chunks.
        /// ASync.
        /// </summary>
        public void ProcessChunks()
        {
            if (pMode != 0)
            {
                return;
            }
            pMode = 1;
            Schedule.StartASyncTask(() =>
            {
                while (true)
                {
                    Thread.Sleep(16);
                    if (pMode == 2)
                    {
                        break;
                    }
                    Schedule.ScheduleSyncTask(() =>
                    {
                        bool ready = true;
                        foreach (Chunk chunk in TheRegion.LoadedChunks.Values)
                        {
                            if (!chunk.PRED)
                            {
                                ready = false;
                                break;
                            }
                        }
                        if (ready)
                        {
                            pMode = 2;
                        }
                    });
                }
                Schedule.ScheduleSyncTask(() =>
                {
                    int c = 0;
                    foreach (Chunk chunk in TheRegion.LoadedChunks.Values)
                    {
                        if (!chunk.PROCESSED)
                        {
                            c++;
                            Schedule.ScheduleSyncTask(() => { chunk.AddToWorld(); }, Utilities.UtilRandom.NextDouble() * 10);
                            Schedule.ScheduleSyncTask(() => { chunk.CreateVBO(); }, Utilities.UtilRandom.NextDouble() * 10);
                            chunk.PROCESSED = true;
                        }
                    }
                    pMode = 0;
                });
            });
        }

        /// <summary>
        /// Returns whether the client is currently on the 'chunk waiting' screen.
        /// </summary>
        public bool IsWaitingOnChunks()
        {
            return CScreen is ChunkWaitingScreen;
        }

        /// <summary>
        /// The "Game" screen.
        /// </summary>
        public GameScreen TheGameScreen;

        /// <summary>
        /// The main menu screen.
        /// </summary>
        MainMenuScreen TheMainMenuScreen;

        /// <summary>
        /// The "singleplayer" main menu screen.
        /// </summary>
        SingleplayerMenuScreen TheSingleplayerMenuScreen;

        /// <summary>
        /// The "waiting on chunks" main menu screen.
        /// </summary>
        public ChunkWaitingScreen TheChunkWaitingScreen;

        /// <summary>
        /// The in-game "inventory" menu screen.
        /// </summary>
        InventoryScreen TheInventoryScreen;

        /// <summary>
        /// The in-game "equipment" menu screen.
        /// </summary>
        EquipmentScreen TheEquipmentScreen;

        /// <summary>
        /// The current sound object for the playing background music.
        /// </summary>
        public ActiveSound CurrentMusic = null;
        
        /// <summary>
        /// Plays the backgrond music. Will restart music if already playing.
        /// </summary>
        void BackgroundMusic()
        {
            if (CurrentMusic != null)
            {
                CurrentMusic.Destroy();
            }
            SoundEffect mus = Sounds.GetSound(CVars.a_music.Value);
            CurrentMusic = Sounds.Play(mus, true, Location.NaN, CVars.a_musicpitch.ValueF, CVars.a_musicvolume.ValueF);
            if (CurrentMusic != null)
            {
                CurrentMusic.IsBackground = true;
            }
        }

        public void onMusicChanged(object obj, EventArgs e)
        {
            BackgroundMusic();
        }

        public void onMusicPitchChanged(object obj, EventArgs e)
        {
            if (CurrentMusic != null)
            {
                // TODO: validate number?
                CurrentMusic.Pitch = CVars.a_musicpitch.ValueF * CVars.a_globalpitch.ValueF;
                CurrentMusic.UpdatePitch();
            }
        }

        public void onMusicVolumeChanged(object obj, EventArgs e)
        {
            if (CurrentMusic != null)
            {
                float vol = CVars.a_musicvolume.ValueF;
                if (vol <= 0 || vol > 1)
                {
                    CurrentMusic.Destroy();
                    CurrentMusic = null;
                }
                else
                {
                    CurrentMusic.Gain = vol;
                    CurrentMusic.UpdateGain();
                }
            }
            else
            {
                BackgroundMusic();
            }
        }
        
        public void UpdateWindow()
        {
            if (CVars.r_width.ValueI < 1280)
            {
                CVars.r_width.Set("1280");
            }
            if (CVars.r_height.ValueI < 720)
            {
                CVars.r_height.Set("720");
            }
            Window.ClientSize = new Size(CVars.r_width.ValueI, CVars.r_height.ValueI);
            Window.WindowState = CVars.r_fullscreen.ValueB ? WindowState.Fullscreen : WindowState.Normal;
            SetViewport();
        }
    }
}
