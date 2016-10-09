//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to crouch.
    /// </summary>
    class MovedownCommand : AbstractCommand
    {
        public Client TheClient;

        public MovedownCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "movedown";
            Description = "Makes the player crouch.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Marker == 0)
            {
                queue.HandleError(entry, "Must use +, -, or !");
            }
            else if (entry.Marker == 1)
            {
                TheClient.Player.Downward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Downward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Downward = !TheClient.Player.Downward;
            }
        }
    }
}
