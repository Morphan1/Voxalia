using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to attack secondarily.
    /// </summary>
    class SecondaryCommand : AbstractCommand
    {
        public Client TheClient;

        public SecondaryCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "secondary";
            Description = "Makes the player attack secondarily.";
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
                TheClient.Player.AltClick = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.AltClick = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.AltClick = !TheClient.Player.AltClick;
            }
        }
    }
}
