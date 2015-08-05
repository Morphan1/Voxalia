using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.CommandSystem.FileCommands
{
    class AddpathCommand: AbstractCommand
    {
        Server TheServer;

        public AddpathCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "addpath";
            Description = "Adds a path to the server file system.";
            Arguments = "<path>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            Program.Files.LoadDir(entry.GetArgument(0));
            entry.Good("Added path.");
        }
    }
}
