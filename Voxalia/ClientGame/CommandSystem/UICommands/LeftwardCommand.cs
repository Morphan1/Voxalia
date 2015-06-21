using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to move leftward.
    /// </summary>
    class LeftwardCommand : AbstractCommand
    {
        public Client TheClient;

        public LeftwardCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "leftward";
            Description = "Moves the player leftward.";
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
                TheClient.Player.Leftward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Leftward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Leftward = !TheClient.Player.Leftward;
            }
        }
    }
}
