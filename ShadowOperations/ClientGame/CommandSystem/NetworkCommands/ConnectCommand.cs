using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.CommandSystem.NetworkCommands
{
    class ConnectCommand: AbstractCommand
    {
        public Client TheClient;

        public ConnectCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "connect";
            Description = "Connects to a server.";
            Arguments = "<ip> <port>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(entry);
                return;
            }
            TheClient.Network.Connect(entry.GetArgument(0), entry.GetArgument(1));
        }
    }
}
