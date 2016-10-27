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
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.GameCommands
{
    public class InventoryCommand: AbstractCommand
    {
        public Client TheClient;

        public InventoryCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "inventory";
            Description = "Opens the inventory screen.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            TheClient.ShowInventory();
        }
    }
}
