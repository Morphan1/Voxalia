using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.ItemSystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsIn
{
    public class HoldItemPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4)
            {
                return false;
            }
            if (Player.Flags.HasFlag(YourStatusFlags.RELOADING))
            {
                // TODO: Send correction packet
                return true; // Permit but ignore
            }
            int dat = Utilities.BytesToInt(data);
            ItemStack old = Player.GetItemForSlot(Player.cItem);
            old.Info.SwitchFrom(Player, old);
            Player.cItem = dat;
            ItemStack newit = Player.GetItemForSlot(Player.cItem);
            newit.Info.SwitchFrom(Player, newit);
            return true;
        }
    }
}
