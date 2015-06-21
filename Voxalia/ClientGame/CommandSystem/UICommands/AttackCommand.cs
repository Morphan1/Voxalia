using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to attack.
    /// </summary>
    class AttackCommand : AbstractCommand
    {
        public Client TheClient;

        public AttackCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "attack";
            Description = "Makes the player attack.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Marker == 0)
            {
                entry.Bad("Must use +, -, or !");
            }
            else if (entry.Marker == 1)
            {
                TheClient.Player.Click = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Click = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Click = !TheClient.Player.Click;
            }
        }
    }
}
