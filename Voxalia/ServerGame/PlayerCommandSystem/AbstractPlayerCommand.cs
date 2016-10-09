//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem
{
    public abstract class AbstractPlayerCommand
    {
        public string Name = null;

        public bool Silent = false;

        public void ShowUsage(string textcat, PlayerCommandEntry entry)
        {
            entry.Player.SendLanguageData(TextChannel.COMMAND_RESPONSE, textcat, "commands.player." + Name + ".description");
            entry.Player.SendLanguageData(TextChannel.COMMAND_RESPONSE, textcat, "commands.player." + Name + ".usage");
        }

        public void ShowUsage(PlayerCommandEntry entry)
        {
            ShowUsage("voxalia", entry);
        }

        public abstract void Execute(PlayerCommandEntry entry);
    }
}
