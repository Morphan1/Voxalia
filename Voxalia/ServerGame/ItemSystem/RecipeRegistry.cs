using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.ItemSystem
{
    public class RecipeRegistry
    {
        public Server TheServer;

        public List<ItemRecipe> Recipes = new List<ItemRecipe>();

        public void AddRecipe(ItemStack result, params ItemStack[] input)
        {
            Recipes.Add(new ItemRecipe() { Result = result, Input = input, TheServer = TheServer });
        }

        public List<ItemRecipe> CanCraftFrom(params ItemStack[] input)
        {
            List<ItemRecipe> items = new List<ItemRecipe>();
            for (int i = 0; i < Recipes.Count; i++)
            {
                if (Recipes[i].Input.Length > input.Length)
                {
                    continue;
                }
                for (int x = 0; x < Recipes[i].Input.Length; x++)
                {
                    if (!HasRequirement(Recipes[i].Input[x], input))
                    {
                        goto rip;
                    }
                }
                items.Add(Recipes[i]);
                rip:
                continue;
            }
            return items;
        }

        public bool HasRequirement(ItemStack requirement, params ItemStack[] input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (Inventory.ItemsMatch(requirement, input[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class ItemRecipe
    {
        public ItemStack Result;
        public ItemStack[] Input;
        public Server TheServer;
    }
}
