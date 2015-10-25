using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ClientGame.ClientMainSystem;
using System.IO;
using System.Threading;
using System.Globalization;
using Voxalia.Shared.Files;

namespace Voxalia
{
    /// <summary>
    /// Central program entry point.
    /// </summary>
    class Program
    {
        /// <summary>
        /// The name of the game.
        /// </summary>
        public static string GameName = "Voxalia";

        /// <summary>
        /// The version of the game.
        /// </summary>
        public static string GameVersion = "0.0.4";

        /// <summary>
        /// A handle for the console window.
        /// </summary>
        public static IntPtr ConsoleHandle;
        
        public static FileHandler Files;

        /// <summary>
        /// Central program entry point.
        /// Decides whether to lauch the server or the client.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        static void Main(string[] args)
        {
            ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            SysConsole.Init();
            StringBuilder arger = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                arger.Append(args[i]).Append(' ');
            }
            try
            {
                Files = new FileHandler();
                Files.Init();
                if (args.Length > 0 && args[0] == "server")
                {
                    Server.Init(arger.ToString().Substring("server".Length).Trim());
                }
                else
                {
                    Client.Init(arger.ToString());
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(ex);
                File.WriteAllText("GLOBALERR_" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt", ex.ToString() + "\n\n" + Environment.StackTrace);
            }
            SysConsole.ShutDown();
            Console.WriteLine("Final shutdown - terminating process.");
            Process.GetCurrentProcess().Kill();
        }
    }
}
