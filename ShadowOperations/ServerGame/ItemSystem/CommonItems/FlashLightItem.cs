using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class FlashLightItem: BaseItemInfo
    {
        public FlashLightItem()
        {
            Name = "flashlight";
        }

        public float Distance = 20;

        public Location Color = Location.One;

        public void Off(PlayerEntity player)
        {
            player.TheServer.SendToAll(new FlashLightPacketOut(player, false, Distance, Color));
            player.FlashLightOn = false;
        }

        public void On(PlayerEntity player)
        {
            player.TheServer.SendToAll(new FlashLightPacketOut(player, true, Distance, Color));
            player.FlashLightOn = true;
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
            On(player);
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
            Off(player);
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
            Off(player);
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }
    }
}
