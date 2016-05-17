using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FreneticScript;
using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.NetworkCommands
{
    public class LoginCommand : AbstractCommand
    {
        public Client TheClient;

        public LoginCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "login";
            Description = "Logs in to the global server.";
            Arguments = "<username> <password>";
            MinimumArguments = 2;
            MaximumArguments = 2;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string username = entry.GetArgument(queue, 0);
            string password = entry.GetArgument(queue, 1);
            if (entry.ShouldShowGood(queue))
            {
                entry.Good(queue, "Attempting login...");
            }
            TheClient.Network.GlobalLoginAttempt(username, password);
        }
    }
}
