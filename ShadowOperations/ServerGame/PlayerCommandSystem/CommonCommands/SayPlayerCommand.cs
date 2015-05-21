using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ServerMainSystem;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.PlayerCommandSystem.CommonCommands
{
    class SayPlayerCommand: AbstractPlayerCommand
    {
        public SayPlayerCommand()
        {
            Name = "say";
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.Network.SendMessage("^r^1/say ^5<message>"); // TODO: ShowUsage
                return;
            }
            DateTime Now = DateTime.Now;
            // TODO: Better format (customizable?)
            entry.Player.TheServer.Broadcast("^r^7[^d^5" + Utilities.Pad(Now.Hour.ToString(), '0', 2, true) + "^7:^5" + Utilities.Pad(Now.Minute.ToString(), '0', 2, true)
                + "^7:^5" + Utilities.Pad(Now.Second.ToString(), '0', 2, true) + "^r^7] <^d" + entry.Player.Name + "^r^7>: ^2^d" + entry.AllArguments());
        }
    }
}
