using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem
{
    public class EntityInventory: Inventory
    {
        public EntityInventory(Region tregion, Entity owner)
            : base(tregion)
        {
            Owner = owner;
        }

        public int cItem = 0;

        public Entity Owner;

        protected override ItemStack GiveItemNoDup(ItemStack item)
        {
            ItemStack it = base.GiveItemNoDup(item);
            it.Info.PrepItem(Owner, it);
            return it;
        }

        public override void RemoveItem(int item)
        {
            item = item % (Items.Count + 1);
            if (item < 0)
            {
                item += Items.Count + 1;
            }
            ItemStack its = GetItemForSlot(item);
            if (item == cItem) // TODO: ensure cItem is wrapped // TODO: should we expect a wrapped cItem from the client and block non-wrapped? Would minimize risks a bit.
            {
                its.Info.SwitchFrom(Owner, its);
            }
            base.RemoveItem(item);
            if (item <= cItem)
            {
                cItem--;
                cItemBack();
            }
        }

        public virtual void cItemBack()
        {
        }
    }
}
