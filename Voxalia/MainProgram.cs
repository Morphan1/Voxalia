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
    class ManProgram
    {
        /// <summary>
        /// A handle for the console window.
        /// </summary>
        public static IntPtr ConsoleHandle;
        
        /// <summary>
        /// Central program entry point.
        /// Decides whether to lauch the server or the client.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        static void Main(string[] args)
        {
            Program.GameName = "Voxalia";
            Program.GameVersion = "0.0.4";
            ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            SysConsole.Init();
            StringBuilder arger = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                arger.Append(args[i]).Append(' ');
            }
            try
            {
                Program.Files = new FileHandler();
                Program.Files.Init();
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
            Environment.Exit(0);
        }
    }
}
