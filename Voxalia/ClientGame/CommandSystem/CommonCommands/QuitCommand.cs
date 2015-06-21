using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.CommonCommands
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

        public override void Execute(CommandEntry entry)
        {
            TheClient.Window.Close();
        }
    }
}
