using System;
using System.Collections.Generic;
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
    class BindblockCommand : AbstractCommand
    {
        public override void AdaptBlockFollowers(CommandEntry entry, List<CommandEntry> input, List<CommandEntry> fblock)
        {
            entry.BlockEnd -= input.Count;
            input.Clear();
            base.AdaptBlockFollowers(entry, input, fblock);
        }

        public Client TheClient;

        public BindblockCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "bindblock";
            Description = "Binds a script block to a key.";
            Arguments = "<key>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(queue, entry);
                return;
            }
            string key = entry.GetArgument(queue, 0);
            if (key == "\0CALLBACK")
            {
                return;
            }
            if (entry.InnerCommandBlock == null)
            {
                queue.HandleError(entry, "Must have a block of commands!");
                return;
            }
            Key k = KeyHandler.GetKeyForName(key);
            KeyHandler.BindKey(k, entry.InnerCommandBlock, entry.BlockStart);
            entry.Good(queue, "Keybind updated for " + KeyHandler.keystonames[k] + ".");
            CommandStackEntry cse = queue.CommandStack.Peek();
            cse.Index = entry.BlockEnd + 2;
        }
    }
}
