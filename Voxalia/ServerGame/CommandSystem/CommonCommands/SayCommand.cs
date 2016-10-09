//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.CommandSystem.CommonCommands
{
    class SayCommand: AbstractCommand
    {
        public Server TheServer;

        public SayCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "say";
            Description = "Says a message to all players on the server.";
            Arguments = "<message>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            DateTime Now = DateTime.Now;
            // TODO: Better format (customizable!)
            TheServer.ChatMessage("^r^7[^d^5" + Utilities.Pad(Now.Hour.ToString(), '0', 2, true) + "^7:^5" + Utilities.Pad(Now.Minute.ToString(), '0', 2, true)
                + "^7:^5" + Utilities.Pad(Now.Second.ToString(), '0', 2, true) + "^r^7] ^3^dSERVER^r^7:^2^d " + entry.AllArguments(queue), "^r^2^d");
        }
    }
}
