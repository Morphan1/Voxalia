//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to quit the game.
    /// </summary>
    class QuitCommand: AbstractCommand
    {
        public Client TheClient;

        public QuitCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "quit";
            Description = "Quits the game.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TheClient.Window.Close();
        }
    }
}
