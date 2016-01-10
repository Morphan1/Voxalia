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
    class UnbindCommand : AbstractCommand
    {
        public Client TheClient;

        public UnbindCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "unbind";
            Description = "Removes any script bound to a key.";
            Arguments = "<key>";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                ShowUsage(entry);
                return;
            }
            string key = entry.GetArgument(0);
            Key k = KeyHandler.GetKeyForName(key);
            KeyHandler.BindKey(k, (string)null);
            entry.Good("Keybind removed.");
        }
    }
}
