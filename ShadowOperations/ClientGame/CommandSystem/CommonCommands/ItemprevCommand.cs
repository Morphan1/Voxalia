using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;
using ShadowOperations.ClientGame.NetworkSystem.PacketsIn;

namespace ShadowOperations.ClientGame.CommandSystem.CommonCommands
{
    /// <summary>
    /// A quick command to switch to the previous item.
    /// </summary>
    class ItemprevCommand : AbstractCommand
    {
        public Client TheClient;

        public ItemprevCommand(Client tclient)
        {
            TheClient = tclient;
            Name = "itemprev";
            Description = "Selects the previous item.";
            Arguments = "";
        }

        public override void Execute(CommandEntry entry)
        {
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            TheClient.QuickBarPos--;
            TheClient.Network.SendPacket(new HoldItemPacketOut(TheClient.QuickBarPos));
            TheClient.RenderExtraItems = 3;
        }
    }
}
