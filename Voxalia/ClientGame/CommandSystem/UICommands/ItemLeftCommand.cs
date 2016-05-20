using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    class ItemleftCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemleftCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemleft";
            Description = "Adjust the item (left version).";
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
                TheClient.Player.ItemLeft = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.ItemLeft = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.ItemLeft = !TheClient.Player.ItemLeft;
            }
        }
    }
}
