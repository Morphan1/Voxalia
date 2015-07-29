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

namespace Voxalia.ClientGame.ClientMainSystem
{
    /// <summary>
    /// The center of all client activity in Shadow Operations.
    /// </summary>
    public partial class Client
    {
        public Server LocalServer = null;

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

        public ClientCommands Commands;

        public ClientCVar CVars;

        public NetworkBase Network;

        public string Username = "Player" + new Random().Next(1000);

        public Texture ItemFrame;

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
            Window = new GameWindow(CVars.r_width.ValueI, CVars.r_height.ValueI, GraphicsMode.Default, Program.GameName + " v" + Program.GameVersion,
                GameWindowFlags.Default, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            Window.WindowBorder = WindowBorder.Fixed;
            Window.Load += new EventHandler<EventArgs>(Window_Load);
            Window.UpdateFrame += new EventHandler<FrameEventArgs>(Window_UpdateFrame);
            Window.RenderFrame += new EventHandler<FrameEventArgs>(Window_RenderFrame);
            Window.Mouse.Move += new EventHandler<MouseMoveEventArgs>(MouseHandler.Window_MouseMove);
            Window.KeyDown += new EventHandler<KeyboardKeyEventArgs>(KeyHandler.PrimaryGameWindow_KeyDown);
            Window.KeyPress += new EventHandler<KeyPressEventArgs>(KeyHandler.PrimaryGameWindow_KeyPress);
            Window.KeyUp += new EventHandler<KeyboardKeyEventArgs>(KeyHandler.PrimaryGameWindow_KeyUp);
            Window.Mouse.WheelChanged += new EventHandler<MouseWheelEventArgs>(KeyHandler.Mouse_Wheel);
            Window.Mouse.ButtonDown += new EventHandler<MouseButtonEventArgs>(KeyHandler.Mouse_ButtonDown);
            Window.Mouse.ButtonUp += new EventHandler<MouseButtonEventArgs>(KeyHandler.Mouse_ButtonUp);
            Window.Closed += new EventHandler<EventArgs>(Window_Closed);
            Window.Run(60, 60);
        }

        void Window_Closed(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        public TextureEngine Textures;
        public ShaderEngine Shaders;
        public GLFontEngine Fonts;
        public FontSetEngine FontSets;
        public Renderer Rendering;
        public ModelEngine Models;
        public AnimationEngine Animations;
        public SoundEngine Sounds;
        public ParticleEngine Particles;
        public TextureBlock TBlock;

        public PlayerEntity Player;

        public string GLVendor;
        public string GLVersion;
        public string GLRenderer;

        void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "Window generated!");
            SysConsole.Output(OutputType.INIT, "Loading textures...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem();
            ItemFrame = Textures.GetTexture("ui/hud/item_frame");
            TBlock = new TextureBlock();
            TBlock.Generate(Textures);
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
            SysConsole.Output(OutputType.INIT, "Loading UI Console...");
            UIConsole.InitConsole();
            SysConsole.Output(OutputType.INIT, "Preparing rendering engine...");
            InitRendering();
            SysConsole.Output(OutputType.INIT, "Loading particle effect engine...");
            Particles = new ParticleEngine(this);
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
            SysConsole.Output(OutputType.INIT, "Setting up screens...");
            TheMainMenuScreen = new MainMenuScreen() { TheClient = this };
            TheGameScreen = new GameScreen() { TheClient = this };
            TheChunkWaitingScreen = new ChunkWaitingScreen() { TheClient = this };
            TheMainMenuScreen.Init();
            TheGameScreen.Init();
            TheChunkWaitingScreen.Init();
            ShowMainMenu();
        }
        
        public void ShowGame()
        {
            if (IsWaitingOnChunks())
            {
                SysConsole.Output(OutputType.INFO, "Solidifying and rendering chunks...");
                TheChunkWaitingScreen.LACS = new LoadAllChunksSystem(TheWorld);
                TheChunkWaitingScreen.LACS.LoadAll(() =>
                {
                    Schedule.ScheduleSyncTask(() =>
                    {
                        // TODO: handle cancel-button cancelling neatly
                        SysConsole.Output(OutputType.INFO, "Showing game...");
                        CScreen = TheGameScreen;
                        CScreen.SwitchTo();
                        TheChunkWaitingScreen.LACS = null;
                    });
                });
            }
            else
            {
                SysConsole.Output(OutputType.INFO, "Showing game...");
                CScreen = TheGameScreen;
                CScreen.SwitchTo();
            }
        }

        public void ShowMainMenu()
        {
            SysConsole.Output(OutputType.INFO, "Showing menu...");
            CScreen = TheMainMenuScreen;
            CScreen.SwitchTo();
        }

        public void ShowChunkWaiting()
        {
            SysConsole.Output(OutputType.INFO, "Awaiting chunks...");
            CScreen = TheChunkWaitingScreen;
            CScreen.SwitchTo();
        }

        byte pMode = 0;

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
                        foreach (Chunk chunk in TheWorld.LoadedChunks.Values)
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
                    foreach (Chunk chunk in TheWorld.LoadedChunks.Values)
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

        public bool IsWaitingOnChunks()
        {
            return CScreen is ChunkWaitingScreen;
        }

        GameScreen TheGameScreen;

        MainMenuScreen TheMainMenuScreen;

        public ChunkWaitingScreen TheChunkWaitingScreen;

        public ActiveSound CurrentMusic = null;

        public string CMusic = "music/epic/bcdenizen";

        void BackgroundMusic()
        {
            if (CurrentMusic != null)
            {
                CurrentMusic.Destroy();
            }
            SoundEffect mus = Sounds.GetSound(CMusic);
            CurrentMusic = Sounds.Play(mus, true, Location.NaN, CVars.a_musicpitch.ValueF, CVars.a_musicvolume.ValueF);
            if (CurrentMusic != null)
            {
                CurrentMusic.IsBackground = true;
            }
        }
    }
}
