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

        public override void Execute(CommandEntry entry)
        {
            if (entry.Arguments.Count < 1)
            {
                entry.Bad("Must specify a slot number!");
                return;
            }
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            TheClient.QuickBarPos = Math.Abs(Utilities.StringToInt(entry.GetArgument(0))) % (TheClient.Items.Count + 1);
            TheClient.Network.SendPacket(new HoldItemPacketOut(TheClient.QuickBarPos));
            TheClient.RenderExtraItems = 3;
        }
    }
}
