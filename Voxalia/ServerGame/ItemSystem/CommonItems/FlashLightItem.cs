using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class FlashLightItem: BaseItemInfo
    {
        public FlashLightItem()
        {
            Name = "flashlight";
        }

        public float Distance = 20;

        public Location Color = Location.One;

        public static void Off(PlayerEntity player)
        {
            player.TheWorld.SendToAll(new FlashLightPacketOut(player, false, 0, Location.Zero));
            player.FlashLightOn = false;
        }

        public void On(PlayerEntity player)
        {
            player.TheWorld.SendToAll(new FlashLightPacketOut(player, true, Distance, Color));
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

        public override void Tick(PlayerEntity player, ItemStack item)
        {
        }
    }
}
