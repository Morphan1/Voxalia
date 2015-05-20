using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class BowItem: BaseItemInfo
    {
        public BowItem()
        {
            Name = "bow";
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public float Speed = 10;

        public override void Click(PlayerEntity player, ItemStack item)
        {
            ArrowEntity ae = new ArrowEntity(player.TheServer);
            ae.SetPosition(player.GetEyePosition());
            ae.NoCollide.Add(player.EID);
            ae.SetVelocity(Utilities.ForwardVector_Deg(player.GetAngles().Yaw, player.GetAngles().Pitch) * 10);
            ae.Angles = player.GetAngles();
            player.TheServer.SpawnEntity(ae);
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(PlayerEntity player, ItemStack item)
        {
        }
    }
}
