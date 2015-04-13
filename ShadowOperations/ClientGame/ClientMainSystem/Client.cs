using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

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

        /// <summary>
        /// Start up and run the server.
        /// </summary>
        public void StartUp()
        {
            SysConsole.Output(OutputType.INIT, "Launching as new client, this is " + (this == Central ? "" : "NOT ") + "the Central client.");
            Window = new GameWindow(800, 600, GraphicsMode.Default, Program.GameName + " v" + Program.GameVersion,
                GameWindowFlags.Default, DisplayDevice.Default, 4, 3, GraphicsContextFlags.ForwardCompatible);
            Window.Load += new EventHandler<EventArgs>(Window_Load);
            Window.UpdateFrame += new EventHandler<FrameEventArgs>(Window_UpdateFrame);
            Window.RenderFrame += new EventHandler<FrameEventArgs>(Window_RenderFrame);
            Window.Run();
        }

        void Window_Load(object sender, EventArgs e)
        {
        }
    }
}
