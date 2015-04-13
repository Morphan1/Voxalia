using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to move downward (crouch).
    /// </summary>
    class DownwardCommand : AbstractCommand
    {
        public Client TheClient;

        public DownwardCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "downward";
            Description = "Moves the player downward (crouches).";
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
