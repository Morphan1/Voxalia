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

        public override void Execute(CommandEntry entry)
        {
            string key = entry.GetArgument(0);
            Key k = KeyHandler.GetKeyForName(key);
            if (entry.Arguments.Count == 1)
            {
                CommandScript cs = KeyHandler.GetBind(k);
                if (cs == null)
                {
                    entry.Bad("That key is not bound, or does not exist.");
                }
                else
                {
                    entry.Info(TagParser.Escape(KeyHandler.keystonames[k] + ": '" + cs.FullString() + "'"));
                }
            }
            else if (entry.Arguments.Count >= 2)
            {
                KeyHandler.BindKey(k, entry.GetArgument(1));
                entry.Good("Keybind updated for " + k + ".");
            }
        }
    }
}
