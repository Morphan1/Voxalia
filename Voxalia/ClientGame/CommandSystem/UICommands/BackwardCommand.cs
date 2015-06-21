using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to move backward.
    /// </summary>
    class BackwardCommand : AbstractCommand
    {
        public Client TheClient;

        public BackwardCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "backward";
            Description = "Moves the player backward.";
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
                TheClient.Player.Backward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Backward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Backward = !TheClient.Player.Backward;
            }
        }
    }
}
