using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.ClientGame.EntitySystem;
using ShadowOperations.ClientGame.GraphicsSystems;
using ShadowOperations.ClientGame.UISystem;
using OpenTK.Input;
using ShadowOperations.ClientGame.CommandSystem;
using System.Diagnostics;
using ShadowOperations.ClientGame.NetworkSystem;
using ShadowOperations.ClientGame.AudioSystem;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    /// <summary>
    /// The center of all client activity in Shadow Operations.
    /// </summary>
    public partial class Client
    {
        /// <summary>
        /// The primary running client.
        /// </summary>
        public static Client Central = null;

        /// <summary>
        /// Starts up a new server.
        /// </summary>
        public static void Init()
        {
            Central = new Client();
            Central.StartUp();
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

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp()
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
            SysConsole.Output(OutputType.INIT, "Generating window...");
            Window = new GameWindow(800, 600, GraphicsMode.Default, Program.GameName + " v" + Program.GameVersion,
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

        public PlayerEntity Player;

        void Window_Load(object sender, EventArgs e)
        {
            SysConsole.Output(OutputType.INIT, "Window generated!");
            SysConsole.Output(OutputType.INIT, "Loading textures...");
            Textures = new TextureEngine();
            Textures.InitTextureSystem();
            ItemFrame = Textures.GetTexture("ui/item_frame");
            SysConsole.Output(OutputType.INIT, "Loading shaders...");
            Shaders = new ShaderEngine();
            string vendor = GL.GetString(StringName.Vendor);
            if (vendor.ToLower().Contains("intel"))
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
            SysConsole.Output(OutputType.INIT, "Preparing mouse and keyboard handlers...");
            MouseHandler.CaptureMouse();
            KeyHandler.Init();
            SysConsole.Output(OutputType.INIT, "Building the sound system...");
            Sounds = new SoundEngine();
            Sounds.Init(CVars);
            SysConsole.Output(OutputType.INIT, "Building physics world...");
            BuildWorld();
            SysConsole.Output(OutputType.INIT, "Spawning the player...");
            Player = new PlayerEntity(this);
            Player.SetPosition(new Location(0, 0, 10));
            SpawnEntity(Player);
            SysConsole.Output(OutputType.INIT, "Preparing networking...");
            Network = new NetworkBase(this);
            SysConsole.Output(OutputType.INIT, "Playing background music...");
            BackgroundMusic();
        }

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
            CurrentMusic.IsBackground = true;
        }
    }
}
