using System.Collections.Generic;
using Voxalia.ServerGame.WorldSystem;
using System.Drawing;
using FreneticScript.TagHandlers;
using Voxalia.Shared;

namespace Voxalia.ServerGame.ItemSystem
{
    public class Inventory
    {
        public WorldSystem.Region TheWorld;

        public Inventory(WorldSystem.Region tregion)
        {
            TheWorld = tregion;
        }

        public List<ItemStack> Items = new List<ItemStack>();

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted.</param>
        /// <returns>A valid item.</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            slot = slot % (Items.Count + 1);
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack("Air", TheWorld.TheServer, 1, "clear", "Air", "An empty slot.", Color.White, "blank.dae", true);
            }
            else
            {
                return Items[slot - 1];
            }
        }

        public int GetSlotForItem(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i] == item)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        public static bool ItemsMatch(ItemStack item, ItemStack item2)
        {
            return item2.Datum == item.Datum &&
                    item2.Name == item.Name &&
                    item2.DisplayName == item.DisplayName &&
                    item2.Description == item.Description &&
                    item2.DrawColor == item.DrawColor &&
                    item2.Image == item.Image &&
                    item2.Model == item.Model &&
                    ItemAttrsMatch(item2, item) &&
                    ItemSharedAttrsMatch(item2, item) &&
                    item2.IsBound == item.IsBound;
            // NOTE: Intentionally don't check the count here.
        }

        public static bool ItemAttrsMatch(ItemStack i1, ItemStack i2)
        {
            if (i1.Attributes.Count != i2.Attributes.Count)
            {
                return false;
            }
            foreach (string str in i1.Attributes.Keys)
            {
                if (!i2.Attributes.ContainsKey(str))
                {
                    return false;
                }
                if (i1.Attributes[str].ToString() != i2.Attributes[str].ToString()) // TODO: Proper tag equality checks?
                {
                    return false;
                }
            }
            return true;
        }


        public static bool ItemSharedAttrsMatch(ItemStack i1, ItemStack i2)
        {
            if (i1.SharedAttributes.Count != i2.SharedAttributes.Count)
            {
                return false;
            }
            foreach (string str in i1.SharedAttributes.Keys)
            {
                if (!i2.SharedAttributes.ContainsKey(str))
                {
                    return false;
                }
                if (i1.SharedAttributes[str] != i2.SharedAttributes[str])
                {
                    return false;
                }
            }
            return true;
        }

        public void GiveItem(ItemStack item)
        {
            GiveItemNoDup(item.Duplicate());
        }

        protected virtual ItemStack GiveItemNoDup(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (ItemsMatch(item, Items[i]))
                {
                    Items[i].Count += item.Count;
                    return Items[i];
                }
            }
            Items.Add(item);
            return item;
        }

        public virtual void RemoveItem(int item)
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
            Items.RemoveAt(item - 1);
        }

    }
}
