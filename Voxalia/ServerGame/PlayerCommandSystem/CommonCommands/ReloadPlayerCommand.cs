﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;

namespace Voxalia.ServerGame.PlayerCommandSystem.CommonCommands
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
            ItemStack item = entry.Player.Items.GetItemForSlot(entry.Player.Items.cItem);
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
