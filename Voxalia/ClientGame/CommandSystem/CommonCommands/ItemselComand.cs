using System;
using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to select an item.
    /// </summary>
    class ItemselCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemselCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemsel";
            Description = "Selects an item to hold by the given number.";
            Arguments = "<slot number>";
        }

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                entry.Bad(queue, "Must specify a slot number!");
                return;
            }
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            int slot = Math.Abs(Utilities.StringToInt(entry.GetArgument(queue, 0))) % (TheClient.Items.Count + 1);
            TheClient.SetHeldItemSlot(slot, DEFAULT_RENDER_EXTRA_ITEMS);
        }

        private const double DEFAULT_RENDER_EXTRA_ITEMS = 3.0;
    }
}
