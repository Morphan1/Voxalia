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
                    entry.Bad(queue, "That key is not bound, or does not exist.");
                }
                else
                {
                    entry.Info(queue, TagParser.Escape(KeyHandler.keystonames[k] + ": {\n" + cs.FullString() + "}"));
                }
            }
            else if (entry.Arguments.Count >= 2)
            {
                KeyHandler.BindKey(k, entry.GetArgument(queue, 1));
                entry.Good(queue, "Keybind updated for " + k + ".");
            }
        }
    }
}
