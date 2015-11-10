using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.UISystem;
using OpenTK.Input;
using Frenetic.TagHandlers;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A quick command to quit the game.
    /// </summary>
    class BindblockCommand : AbstractCommand
    {
        public Client TheClient;

        public BindblockCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "bindblock";
            Description = "Binds a script block to a key.";
            Arguments = "<key>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            if (entry.Block == null)
            {
                entry.Bad("Must have a block of commands!");
                return;
            }
            string key = entry.GetArgument(0);
            Key k = KeyHandler.GetKeyForName(key);
            KeyHandler.BindKey(k, entry.Block);
            entry.Good("Keybind updated.");
        }
    }
}
