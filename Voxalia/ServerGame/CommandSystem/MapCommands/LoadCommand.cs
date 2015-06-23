using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;
using System.Diagnostics;

namespace Voxalia.ServerGame.CommandSystem.MapCommands
{
    class LoadCommand: AbstractCommand
    {
        public Server TheServer;

        public LoadCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "load";
            Description = "Loads a map from file";
            Arguments = "<map name>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string mapname = entry.GetArgument(0);
            Stopwatch sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            if (!Program.Files.Exists("maps/" + mapname + ".map"))
            {
                entry.Bad("Invalid map name.");
                return;
            }
            string data = Program.Files.ReadText("maps/" + mapname + ".map");
            TheServer.LoadedWorlds[0].LoadMapFromString(data); // TODO: Choose world better. Generate, ignore, etc.
            sw.Stop();
            entry.Good("Loaded map in " + (float)sw.ElapsedMilliseconds / 1000f + " seconds.");
        }
    }
}
