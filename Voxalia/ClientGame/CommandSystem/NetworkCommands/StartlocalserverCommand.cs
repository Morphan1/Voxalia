using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ServerGame.ServerMainSystem;
using System.Threading.Tasks;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
{
    public class StartlocalserverCommand: AbstractCommand
    {
        Client TheClient;

        public StartlocalserverCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "startlocalserver";
            Description = "Launches you into a local game server";
            Arguments = "[port]";
        }

        public override void Execute(CommandEntry entry)
        {
            string arg0 = "28010";
            if (entry.Arguments.Count >= 1)
            {
                arg0 = entry.GetArgument(0);
            }
            if (TheClient.LocalServer != null)
            {
                entry.Good("Shutting down pre-existing server.");
                TheClient.LocalServer.ShutDown();
                TheClient.LocalServer = null;
            }
            entry.Good("Generating new server...");
            TheClient.LocalServer = new Server();
            Server.Central = TheClient.LocalServer;
            Task.Factory.StartNew(() => { TheClient.LocalServer.StartUp(); });
        }
    }
}
