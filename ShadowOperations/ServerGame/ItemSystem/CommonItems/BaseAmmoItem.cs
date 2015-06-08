using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public abstract class BaseAmmoItem : BaseItemInfo
    {
        public BaseAmmoItem(string name)
        {
            Name = name;
        }

        public override void PrepItem(PlayerEntity player, ItemStack item)
        {
        }

        public override void Click(PlayerEntity player, ItemStack item)
        {
        }

        public override void AltClick(PlayerEntity player, ItemStack item)
        {
        }

        public override void ReleaseClick(PlayerEntity player, ItemStack item)
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
