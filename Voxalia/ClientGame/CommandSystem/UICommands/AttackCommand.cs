using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    /// <summary>
    /// A command to attack.
    /// </summary>
    class AttackCommand : AbstractCommand
    {
        public Client TheClient;

        public AttackCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "attack";
            Description = "Makes the player attack.";
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
                TheClient.Player.Click = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.Click = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.Click = !TheClient.Player.Click;
            }
        }
    }
}
