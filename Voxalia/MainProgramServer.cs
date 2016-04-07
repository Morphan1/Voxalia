using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
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
        /// <param name="args">The command line arguments.</param>
        static void Main(string[] args)
        {
            ConsoleHandle = Process.GetCurrentProcess().MainWindowHandle;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
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
                Program.Init();
                Server.Init(arger.ToString());
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
