using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 2)
            {
                ShowUsage(queue, entry);
                return;
            }
            TheClient.Network.Connect(entry.GetArgument(queue, 0), entry.GetArgument(queue, 1));
        }
    }
}
