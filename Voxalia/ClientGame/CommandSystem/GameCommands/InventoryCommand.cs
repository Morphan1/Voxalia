using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
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

        public override void Execute(CommandEntry entry)
        {
            entry.Good("Inventory!");
        }
    }
}
