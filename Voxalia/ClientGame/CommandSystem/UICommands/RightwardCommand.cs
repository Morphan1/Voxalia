using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to move rightward.
    /// </summary>
    class RightwardCommand : AbstractCommand
    {
        public Client TheClient;

        public RightwardCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "rightward";
            Description = "Moves the player rightward.";
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
                TheClient.Player.Rightward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Rightward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Rightward = !TheClient.Player.Rightward;
            }
        }
    }
}
