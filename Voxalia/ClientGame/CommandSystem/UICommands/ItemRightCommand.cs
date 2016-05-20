using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    class ItemrightCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemrightCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemright";
            Description = "Adjust the item (right version).";
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
                TheClient.Player.ItemRight = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.ItemRight = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.ItemRight = !TheClient.Player.ItemRight;
            }
        }
    }
}
