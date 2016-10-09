//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
        public Server TheServer;

        // TDOO: ClearPaths command

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
            TheServer.Files.LoadDir(entry.GetArgument(queue, 0));
            entry.Good(queue, "Added path.");
        }
    }
}
