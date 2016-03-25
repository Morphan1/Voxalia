using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ServerGame.ServerMainSystem;
using System.Threading.Tasks;
using Voxalia.Shared;

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
            Waitable = true;
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
            Action callback = null;
            if (entry.WaitFor && entry.Queue.WaitingOn == entry)
            {
                callback = () =>
                {
                    entry.Queue.WaitingOn = null;
                };
            }
            Task.Factory.StartNew(() =>
            {
                try
                {
                    TheClient.LocalServer.StartUp(callback);
                }
                catch (Exception ex)
                {
                    SysConsole.Output("Running local server", ex);
                }
            });
        }
    }
}
