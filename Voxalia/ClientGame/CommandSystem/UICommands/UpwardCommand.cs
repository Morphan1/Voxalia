using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to move upward (jump).
    /// </summary>
    class UpwardCommand : AbstractCommand
    {
        public Client TheClient;

        public UpwardCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "upward";
            Description = "Moves the player upward (jumps).";
            Arguments = "";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Marker == 0)
            {
                queue.HandleError(entry, "Must use +, -, or !");
            }
            else if (entry.Marker == 1)
            {
                TheClient.Player.Upward = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Upward = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Upward = !TheClient.Player.Upward;
            }
        }
    }
}
