using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.ItemSystem;
using ShadowOperations.ServerGame.ItemSystem.CommonItems;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

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
                BaseGunItem gun = (BaseGunItem) item.Info;
                if (item.Datum < gun.ClipSize)
                {
                    ItemStack ammo = null;
                    foreach (ItemStack itemStack in entry.Player.Items)
                    {
                        if (itemStack.Name == "bullet" && itemStack.SecondaryName == gun.AmmoType)
                        {
                            ammo = itemStack;
                            break;
                        }
                    }
                    if (ammo != null && ammo.Count > 0)
                    {
                        int reloading = gun.ClipSize - item.Datum;
                        if (reloading > ammo.Count)
                        {
                            reloading = ammo.Count;
                        }
                        item.Datum += reloading;
                        ammo.Count -= reloading;
                        entry.Player.Network.SendPacket(new SetItemPacketOut(entry.Player.Items.IndexOf(ammo), ammo));
                    }
                }
            }
        }
    }
}
