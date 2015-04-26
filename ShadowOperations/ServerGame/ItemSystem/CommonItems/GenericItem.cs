using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.ItemSystem.CommonItems
{
    public class GenericItem: BaseItemInfo
    {
        public override void PrepItem(EntitySystem.PlayerEntity player, ItemStack item)
        {
            Name = "Err";
        }

        public override void Click(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void AltClick(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void Use(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchFrom(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }

        public override void SwitchTo(EntitySystem.PlayerEntity player, ItemStack item)
        {
        }
    }
}
