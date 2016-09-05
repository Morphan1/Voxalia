using System;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;
using Voxalia.Shared;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
{
    class WeaponreloadPlayerCommand : AbstractPlayerCommand
    {
        public WeaponreloadPlayerCommand()
        {
            Name = "weaponreload";
            Silent = true;
        }

        public override void Execute(PlayerCommandEntry entry)
        {
            ItemStack item = entry.Player.Items.GetItemForSlot(entry.Player.Items.cItem);
            if (item.Info is BaseGunItem)
            {
                ((BaseGunItem)item.Info).Reload(entry.Player, item);
            }
            else if (item.Info is BowItem)
            {
                entry.Player.ItemStartClickTime = -2;
            }
            else
            {
                entry.Player.SendMessage(TextChannel.COMMAND_RESPONSE, "You can't reload that."); // TODO: Language, etc.
            }
        }
    }
}
