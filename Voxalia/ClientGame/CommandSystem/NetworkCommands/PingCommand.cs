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
            string ip = entry.GetArgument(queue, 0);
            string port = entry.GetArgument(queue, 1);
            TheClient.Network.Ping(ip, port, (info) =>
            {
                if (info.Success)
                {
                    UIConsole.WriteLine("^r^2Ping success(" + ip + " " + port + "): " + info.Message + " (" + info.Ping + "ms)");
                }
                else
                {
                    UIConsole.WriteLine("^r^1Ping failure (" + ip + " " + port + "): " + info.Message);
                }
            });
        }
    }
}
