using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using System.Drawing;

namespace Voxalia.ServerGame.ItemSystem
{
    public class Inventory
    {
        public World TheWorld;

        public Inventory(World tworld)
        {
            TheWorld = tworld;
        }

        public List<ItemStack> Items = new List<ItemStack>();

        /// <summary>
        /// Returns an item in the quick bar.
        /// Can return air.
        /// </summary>
        /// <param name="slot">The slot, any number is permitted</param>
        /// <returns>A valid item</returns>
        public ItemStack GetItemForSlot(int slot)
        {
            while (slot < 0)
            {
                slot += Items.Count + 1;
            }
            while (slot > Items.Count)
            {
                slot -= Items.Count + 1;
            }
            if (slot == 0)
            {
                return new ItemStack("Air", TheWorld.TheServer, 1, "clear", "Air", "An empty slot.", Color.White.ToArgb(), "blank.dae", true);
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

        public bool ItemAttrsMatch(ItemStack i1, ItemStack i2)
        {
            Dictionary<string, string>.KeyCollection keys1 = i1.Attributes.Keys;
            Dictionary<string, string>.KeyCollection keys2 = i2.Attributes.Keys;
            if (keys1.Count != keys2.Count)
            {
                return false;
            }
            foreach (string str in keys1)
            {
                if (i1.Attributes[str] != i2.Attributes[str])
                {
                    return false;
                }
            }
            return true;
        }

        public virtual ItemStack GiveItem(ItemStack item)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                if (Items[i].Datum == item.Datum &&
                    Items[i].Name == item.Name &&
                    Items[i].DisplayName == item.DisplayName &&
                    Items[i].Description == item.Description &&
                    Items[i].DrawColor == item.DrawColor &&
                    Items[i].Image == item.Image &&
                    Items[i].Model == item.Model &&
                    ItemAttrsMatch(Items[i], item) &&
                    Items[i].IsBound == item.IsBound)
                    // TODO: Better match logic
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
