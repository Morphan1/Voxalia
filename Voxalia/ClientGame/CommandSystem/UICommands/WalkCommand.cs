using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to walk.
    /// </summary>
    class WalkCommand : AbstractCommand
    {
        public Client TheClient;

        public WalkCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "walk";
            Description = "Makes the player walk.";
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
                TheClient.Player.Walk = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Walk = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Walk = !TheClient.Player.Walk;
            }
        }
    }
}
