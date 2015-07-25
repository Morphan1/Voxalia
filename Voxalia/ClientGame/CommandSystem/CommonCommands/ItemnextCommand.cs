using Frenetic.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to switch to the next item.
    /// </summary>
    class ItemnextCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemnextCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemnext";
            Description = "Selects the next item.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            TheClient.QuickBarPos++;
            TheClient.Network.SendPacket(new HoldItemPacketOut(TheClient.QuickBarPos));
            TheClient.RenderExtraItems = 3;
        }
    }
}
