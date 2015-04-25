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
            SysConsole.Output(OutputType.INFO, "Preparing a generic item for " + player + ", with item " + item);
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
    }
}
