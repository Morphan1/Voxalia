//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.CommandSystem.CommonCommands
{
    public class QuitCommand: AbstractCommand
    {
        public Server TheServer;

        public QuitCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "quit";
            Description = "Closes the server entirely.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TheServer.ShutDown();
        }
    }
}
