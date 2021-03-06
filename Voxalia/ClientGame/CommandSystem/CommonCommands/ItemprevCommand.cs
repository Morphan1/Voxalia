//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.CommandSystem;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.NetworkSystem.PacketsOut;
using Voxalia.Shared;

namespace Voxalia.ClientGame.CommandSystem.CommonCommands
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

        public override void Execute(CommandQueue queue, CommandEntry entry)
        {
            if (TheClient.Player.ServerFlags.HasFlag(YourStatusFlags.RELOADING))
            {
                return;
            }
            TheClient.QuickBarPos--;
            while (TheClient.QuickBarPos < 0)
            {
                TheClient.QuickBarPos += TheClient.Items.Count + 1;
            }
            TheClient.Network.SendPacket(new HoldItemPacketOut(TheClient.QuickBarPos));
            TheClient.RenderExtraItems = 3;
        }
    }
}
