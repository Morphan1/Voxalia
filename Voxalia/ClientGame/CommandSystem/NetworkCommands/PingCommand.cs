using FreneticScript.CommandSystem;
using System;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.UISystem;

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
            TheClient.Network.Ping(entry.GetArgument(queue, 0), entry.GetArgument(queue, 1), (info) =>
            {
                if (info.success)
                {
                    UIConsole.WriteLine("^r^2" + info.motd + " (" + info.ping + "ms)");
                }
                else
                {
                    UIConsole.WriteLine("^r^1" + info.motd);
                }
            });
        }
    }
}
