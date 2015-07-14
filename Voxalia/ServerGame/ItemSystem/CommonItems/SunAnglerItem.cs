using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem.CommonItems
{
    public class SunAnglerItem: BaseItemInfo
    {
        public SunAnglerItem()
        {
            Name = "sun_angler";
        }
        
        public override void PrepItem(Entity entity, ItemStack item)
        {
        }

        public override void Click(Entity entity, ItemStack item)
        {
            if (!(entity is PlayerEntity))
            {
                // TODO: Work with non-players!
                return;
            }
            PlayerEntity player = (PlayerEntity)entity;
            player.TheWorld.SendToAll(new SunAnglePacketOut(180f + (float)player.Direction.Yaw, (float)-player.Direction.Pitch));
        }

        public override void AltClick(Entity entity, ItemStack item)
        {
        }

        public override void ReleaseClick(Entity entity, ItemStack item)
        {
        }

        public override void Use(Entity entity, ItemStack item)
        {
        }

        public override void SwitchFrom(Entity entity, ItemStack item)
        {
        }

        public override void SwitchTo(Entity entity, ItemStack item)
        {
        }

        public override void Tick(Entity entity, ItemStack item)
        {
        }
    }
}
