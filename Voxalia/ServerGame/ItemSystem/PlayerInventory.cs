using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.ItemSystem
{
    public class PlayerInventory: EntityInventory
    {
        public PlayerInventory(PlayerEntity player)
            : base(player.TheWorld, player)
        {
        }

        public override ItemStack GiveItem(ItemStack item)
        {
            ItemStack it = base.GiveItem(item);
            if (it == item)
            {
                ((PlayerEntity)Owner).Network.SendPacket(new SpawnItemPacketOut(Items.Count - 1, it));
            }
            else
            {
                ((PlayerEntity)Owner).Network.SendPacket(new SetItemPacketOut(GetSlotForItem(it) - 1, it));
            }
            return it;
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
