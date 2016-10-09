//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
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
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "^r^1/say ^5<message>"); // TODO: ShowUsage
                return;
            }
            string message = entry.AllArguments();
            if (entry.Player.TheServer.CVars.t_blockcolors.ValueB)
            {
                message = message.Replace("^", "^^n");
            }
            DateTime Now = DateTime.Now; // TODO: Retrieve time of server current tick, as opposed to actual current time.
            // TODO: Better format (customizable!)
            entry.Player.TheServer.ChatMessage("^r^7[^d^5" + Utilities.Pad(Now.Hour.ToString(), '0', 2, true) + "^7:^5" + Utilities.Pad(Now.Minute.ToString(), '0', 2, true)
                + "^7:^5" + Utilities.Pad(Now.Second.ToString(), '0', 2, true) + "^r^7] <^d" + entry.Player.Name + "^r^7>:^2^d " + message, "^r^2^d");
        }
    }
}
