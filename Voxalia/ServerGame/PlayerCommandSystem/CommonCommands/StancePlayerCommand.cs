//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using BEPUphysics.Character;
using FreneticScript;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    public class StancePlayerCommand: AbstractPlayerCommand
    {
        public StancePlayerCommand()
        {
            Name = "stance";
            Silent = true;
        }
        
        // TODO: Clientside?
        public override void Execute(PlayerCommandEntry entry)
        {
            if (entry.InputArguments.Count < 1)
            {
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "^r^1/stance <stance>"); // TODO: ShowUsage
                return;
            }
            string stance = entry.InputArguments[0].ToLowerFast();
            // TOOD: Implement!
            if (stance == "stand")
            {
                entry.Player.DesiredStance = Stance.Standing;
            }
            else if (stance == "crouch")
            {
                entry.Player.DesiredStance = Stance.Crouching;
            }
            else
            {
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "^r^1Unknown stance input."); // TODO: Languaging
            }
        }
    }
}
