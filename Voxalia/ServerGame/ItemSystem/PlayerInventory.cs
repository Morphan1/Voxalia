﻿using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem
{
    public class PlayerInventory: EntityInventory
    {
        public PlayerInventory(PlayerEntity player)
            : base(player.TheRegion, player)
        {
        }

        protected override ItemStack GiveItemNoDup(ItemStack item)
        {
            // TODO: Better handling method here.
            ItemStack it = base.GiveItemNoDup(item);
            if (ReferenceEquals(it, item))
            {
                ((PlayerEntity)Owner).Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, it));
            }
            else
            {
                ((PlayerEntity)Owner).Network.SendPacket(new SetItemPacketOut(GetSlotForItem(it) - 1, it));
            }
            return it;
        }

        public override void SetSlot(int slot, ItemStack item)
        {
            base.SetSlot(slot, item);
            ((PlayerEntity)Owner).Network.SendPacket(new SetItemPacketOut(slot, item));
        }

        public override void RemoveItem(int item)
        {
            base.RemoveItem(item);
            ((PlayerEntity)Owner).Network.SendPacket(new RemoveItemPacketOut(item - 1));
        }

        public override void cItemBack()
        {
            ((PlayerEntity)Owner).Network.SendPacket(new SetHeldItemPacketOut(cItem));
        }
    }
}
