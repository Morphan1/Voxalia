using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;

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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            Program.Files.LoadDir(entry.GetArgument(queue, 0));
            entry.Good(queue, "Added path.");
        }
    }
}
