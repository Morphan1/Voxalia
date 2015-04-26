using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.NetworkCommands
{
    class DisconnectCommand: AbstractCommand
    {
        public Client TheClient;

        public DisconnectCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "disconnect";
            Description = "Disconnects from the server.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            TheClient.Network.Disconnect();
        }
    }
}
