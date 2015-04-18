using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.CommandSystem.MapCommands
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
            if (!FileHandler.Exists("maps/" + mapname + ".map"))
            {
                entry.Bad("Invalid map name.");
                return;
            }
            string data = FileHandler.ReadText("maps/" + mapname + ".map");
            TheServer.LoadMapFromString(data);
            entry.Good("Loaded map.");
        }
    }
}
