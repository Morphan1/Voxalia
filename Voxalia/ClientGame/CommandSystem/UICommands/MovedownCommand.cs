using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to crouch.
    /// </summary>
    class MovedownCommand : AbstractCommand
    {
        public Client TheClient;

        public MovedownCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "movedown";
            Description = "Makes the player crouch.";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Marker == 0)
            {
                entry.Bad(queue, "Must use +, -, or !");
            }
            else if (entry.Marker == 1)
            {
                TheClient.Player.Downward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Downward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Downward = !TheClient.Player.Downward;
            }
        }
    }
}
