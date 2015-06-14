using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.ItemSystem
{
    public abstract class BaseItemInfo
    {
        public string Name;

        public abstract void PrepItem(PlayerEntity player, ItemStack item);

        public abstract void Click(PlayerEntity player, ItemStack item);

        public abstract void AltClick(PlayerEntity player, ItemStack item);

        public abstract void ReleaseClick(PlayerEntity player, ItemStack item);

        public abstract void Use(PlayerEntity player, ItemStack item);

        public abstract void SwitchFrom(PlayerEntity player, ItemStack item);

        public abstract void SwitchTo(PlayerEntity player, ItemStack item);

        public abstract void Tick(PlayerEntity player, ItemStack item);
    }
}
