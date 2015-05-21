using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic;
using Frenetic.CommandSystem;
using ShadowOperations.ServerGame.ServerMainSystem;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Common;
using Frenetic.TagHandlers.Objects;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.CommandSystem.CommonCommands
{
    class SayCommand: AbstractCommand
    {
        public Server TheServer;

        public SayCommand(Server tserver)
        {
            TheServer = tserver;
            Name = "say";
            Description = "Says a message to all players on the server.";
            Arguments = "<message>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            DateTime Now = DateTime.Now;
            // TODO: Better format (customizable?)
            TheServer.Broadcast("^r^7[^d^5" + Utilities.Pad(Now.Hour.ToString(), '0', 2, true) + "^7:^5" + Utilities.Pad(Now.Minute.ToString(), '0', 2, true)
                + "^7:^5" + Utilities.Pad(Now.Second.ToString(), '0', 2, true) + "^r^7] ^3^dSERVER^r^7: ^2^d" + entry.AllArguments());
        }
    }
}
