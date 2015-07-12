using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.ItemSystem
{
    public class EntityInventory: Inventory
    {
        public EntityInventory(World tworld, Entity owner)
            : base(tworld)
        {
            Owner = owner;
        }

        public int cItem = 0;

        public Entity Owner;

        public override ItemStack GiveItem(ItemStack item)
        {
            ItemStack it = base.GiveItem(item);
            // TODO: if it == item?
            it.Info.PrepItem(Owner, it);
            return it;
        }

        public override void RemoveItem(int item)
        {
            while (item < 0)
            {
                item += Items.Count + 1;
            }
            while (item > Items.Count)
            {
                item -= Items.Count + 1;
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
