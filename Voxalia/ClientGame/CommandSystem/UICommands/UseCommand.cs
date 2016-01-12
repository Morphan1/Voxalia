using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to use things.
    /// </summary>
    class UseCommand : AbstractCommand
    {
        public Client TheClient;

        public UseCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "use";
            Description = "Makes the player use things.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            if (entry.Marker == 0)
            {
                entry.Bad("Must use +, -, or !");
            }
            else if (entry.Marker == 1)
            {
                TheClient.Player.Use = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Use = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Use = !TheClient.Player.Use;
            }
        }
    }
}
