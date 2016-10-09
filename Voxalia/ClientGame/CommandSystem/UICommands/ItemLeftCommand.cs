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
    class ItemleftCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemleftCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemleft";
            Description = "Adjust the item (left version).";
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
                TheClient.Player.ItemLeft = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.ItemLeft = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.ItemLeft = !TheClient.Player.ItemLeft;
            }
        }
    }
}
