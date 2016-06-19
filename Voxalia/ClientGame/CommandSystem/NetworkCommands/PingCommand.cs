using FreneticScript.CommandSystem;
using System;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
{
    class PingCommand: AbstractCommand
    {
        public Client TheClient;

        public PingCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "ping";
            Description = "Pings a a server.";
            Arguments = "<ip> <port>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(queue, entry);
                return;
            }
            try {
                string response = TheClient.Network.Ping(entry.GetArgument(queue, 0), entry.GetArgument(queue, 1));
                entry.Good(queue, response);
            }
            catch (Exception e)
            {
                entry.Bad(queue, e.Message);
            }
        }
    }
}
