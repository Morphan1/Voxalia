using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.CommandSystem;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ClientGame.CommandSystem.CommonCommands
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
            TheClient.QuickBarPos++;
            TheClient.Network.SendPacket(new HoldItemPacketOut(TheClient.QuickBarPos));
        }
    }
}
