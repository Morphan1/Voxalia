using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.CommandSystem.UICommands
{
    class ItemupCommand : AbstractCommand
    {
        public Client TheClient;
        
        public ItemupCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemup";
            Description = "Adjust the item (up version).";
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
                TheClient.Player.ItemUp = true;
            }
            else if (entry.Marker == 2)
            {
                TheClient.Player.ItemUp = false;
            }
            else if (entry.Marker == 3)
            {
                TheClient.Player.ItemUp = !TheClient.Player.ItemUp;
            }
        }
    }
}
