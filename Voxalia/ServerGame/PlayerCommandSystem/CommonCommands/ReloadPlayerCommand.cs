using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ItemSystem;
using ShadowOperations.ServerGame.ItemSystem.CommonItems;

namespace ShadowOperations.ServerGame.PlayerCommandSystem.CommonCommands
{
    class ReloadPlayerCommand : AbstractPlayerCommand
    {
        public ReloadPlayerCommand()
        {
            Name = "reload";
            Silent = true;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            ItemStack item = entry.Player.GetItemForSlot(entry.Player.cItem);
            if (item.Info is BaseGunItem)
            {
                ((BaseGunItem)item.Info).Reload(entry.Player, item);
            }
            else
            {
                entry.Player.Network.SendMessage("You can't reload that."); // TODO: Language, etc.
            }
        }
    }
}
