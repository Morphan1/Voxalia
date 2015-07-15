using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem
{
    public abstract class BaseItemInfo
    {
        public string Name;

        public abstract void PrepItem(Entity player, ItemStack item);

        public abstract void Click(Entity player, ItemStack item);

        public abstract void AltClick(Entity player, ItemStack item);

        public abstract void ReleaseClick(Entity player, ItemStack item);

        public abstract void ReleaseAltClick(Entity player, ItemStack item);

        public abstract void Use(Entity player, ItemStack item);

        public abstract void SwitchFrom(Entity player, ItemStack item);

        public abstract void SwitchTo(Entity player, ItemStack item);

        public abstract void Tick(Entity player, ItemStack item);
    }
}
