using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ClientGame.ClientMainSystem;
using System.IO;

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
        public static string GameVersion = "0.0.2";

        /// <summary>
        /// A handle for the console window.
        /// </summary>
        public static IntPtr ConsoleHandle;

        /// <summary>
        /// All -flags in the command line input.
        /// </summary>
        public static List<char> LaunchFlags;

        /// <summary>
        /// All --settings in the command line input.
        /// </summary>
        public static List<KeyValuePair<string, string>> LaunchSettings;

        /// <summary>
        /// Gets the value of a setting.
        /// </summary>
        /// <param name="setting">The setting to get the value of</param>
        /// <returns>The setting's value</returns>
        public static string GetSetting(string setting)
        {
            for (int i = 0; i < LaunchSettings.Count; i++)
            {
                if (LaunchSettings[i].Key.ToLower() == setting)
                {
                    return LaunchSettings[i].Value;
                }
            }
            return null;
        }

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
            LaunchFlags = new List<char>();
            LaunchSettings = new List<KeyValuePair<string, string>>();
            bool in_setting = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("--") && args[i].Length > 2)
                {
                    in_setting = true;
                    Console.WriteLine("Found setting " + args[i].Substring(2));
                    LaunchSettings.Add(new KeyValuePair<string, string>(args[i].Substring(2), ""));
                }
                else if (args[i].StartsWith("-") && args[i].Length > 1)
                {
                    for (int x = 1; x < args[i].Length; x++)
                    {
                        Console.WriteLine("Found flag " + args[i][x]);
                        LaunchFlags.Add(args[i][x]);
                    }
                    in_setting = false;
                }
                else if (in_setting)
                {
                    KeyValuePair<string, string> kvp = LaunchSettings[LaunchSettings.Count - 1];
                    LaunchSettings.RemoveAt(LaunchSettings.Count - 1);
                    Console.WriteLine("Found setting argument, " + kvp.Key + " is now " + kvp.Value + (kvp.Value.Length > 0 ? " " : "") + args[i]);
                    LaunchSettings.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value + (kvp.Value.Length > 0 ? " " : "") + args[i]));
                }
            }
            try
            {
                Files = new FileHandler();
                Files.Init();
                if (args.Length > 0 && args[0] == "server")
                {
                    Server.Init();
                }
                else
                {
                    Client.Init();
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(ex);
                File.WriteAllText("GLOBALERR_" + DateTime.Now.ToFileTimeUtc().ToString() + ".txt", ex.GetType().Name + ": " + ex.Message + "\n" + Environment.StackTrace + "\n\n" + ex.StackTrace.ToString());
            }
            SysConsole.ShutDown();
        }
    }
}
