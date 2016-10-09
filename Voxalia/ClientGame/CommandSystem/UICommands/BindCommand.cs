//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.UISystem;
using OpenTK.Input;
using FreneticScript.TagHandlers;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A quick command to quit the game.
    /// </summary>
    class BindCommand : AbstractCommand
    {
        public Client TheClient;

        public BindCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "bind";
            Description = "Binds a script to a key.";
            Arguments = "<key> [binding]";
            MinimumArguments = 1;
            MaximumArguments = 2;
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            string key = entry.GetArgument(queue, 0);
            Key k = KeyHandler.GetKeyForName(key);
            if (entry.Arguments.Count == 1)
            {
                CommandScript cs = KeyHandler.GetBind(k);
                if (cs == null)
                {
                    queue.HandleError(entry, "That key is not bound, or does not exist.");
                }
                else
                {
                    entry.Info(queue, TagParser.Escape(KeyHandler.keystonames[k] + ": {\n" + cs.FullString() + "}"));
                }
            }
            else if (entry.Arguments.Count >= 2)
            {
                KeyHandler.BindKey(k, entry.GetArgument(queue, 1));
                entry.Good(queue, "Keybind updated for " + KeyHandler.keystonames[k] + ".");
            }
        }
    }
}
