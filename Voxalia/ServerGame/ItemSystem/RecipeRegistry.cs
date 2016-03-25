using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.CommandSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.ServerMainSystem;
using FreneticScript;

namespace Voxalia.ServerGame.ItemSystem
{
    public class RecipeRegistry
    {
        public Server TheServer;

        public List<ItemRecipe> Recipes = new List<ItemRecipe>();

        public void AddRecipe(RecipeMode mode, List<CommandEntry> determiner, int adj, params ItemStack[] input)
        {
            Recipes.Add(new ItemRecipe() { Mode = mode, Determinant = new CommandScript("recipe_" + Recipes.Count, determiner, adj), Input = input, TheServer = TheServer, Index = Recipes.Count });
        }

        public List<RecipeResult> CanCraftFrom(params ItemStack[] input)
        {
            ItemStack[] inpitems = new ItemStack[input.Length];
            input.CopyTo(inpitems, 0);
            List<RecipeResult> items = new List<RecipeResult>();
            for (int i = 0; i < Recipes.Count; i++)
            {
                if (Recipes[i].Input.Length > input.Length)
                {
                    continue;
                }
                List<ItemStack> usedhere = new List<ItemStack>();
                for (int x = 0; x < Recipes[i].Input.Length; x++)
                {
                    int used;
                    if (!HasRequirement(Recipes[i].Mode, Recipes[i].Input[x], inpitems, out used))
                    {
                        goto rip;
                    }
                    inpitems[used] = null;
                    ItemStack usednow = input[i].Duplicate();
                    usednow.Count = Recipes[i].Input[x].Count;
                    usedhere.Add(usednow);
                }
                items.Add(new RecipeResult() { Recipe = Recipes[i], UsedInput = usedhere });
                rip:
                continue;
            }
            return items;
        }

        public static RecipeMode ModeFor(ListTag modeinput)
        {
            if (modeinput.ListEntries.Count == 1 && modeinput.ListEntries[0].ToString().ToLowerFast() == "strict")
            {
                return RecipeMode.BOUND | RecipeMode.COLOR | RecipeMode.DATUM | RecipeMode.DESCRIPTION | RecipeMode.DISPLAY | RecipeMode.LOCAL
                    | RecipeMode.MODEL | RecipeMode.SECONDARY | RecipeMode.SHARED | RecipeMode.TEXTURE | RecipeMode.TYPE;
            }
            RecipeMode mode = 0;
            foreach (TemplateObject obj in modeinput.ListEntries)
            {
                RecipeMode adder;
                if (Enum.TryParse(obj.ToString().ToUpperInvariant(), out adder))
                {
                    mode |= adder;
                }
            }
            return mode;
        }

        public bool HasRequirement(RecipeMode mode, ItemStack requirement, ItemStack[] input, out int used)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == null)
                {
                    continue;
                }
                bool thisone = true;
                if (input[i].Count < requirement.Count)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.TYPE) && requirement.Name != input[i].Name)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.SECONDARY) && requirement.SecondaryName != input[i].SecondaryName)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.TEXTURE) && requirement.GetTextureName() != input[i].GetTextureName())
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.MODEL) && requirement.GetModelName() != input[i].GetModelName())
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.MODEL) && requirement.GetModelName() != input[i].GetModelName())
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.SHARED) && !Inventory.ItemSharedAttrsMatch(requirement, input[i]))
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.LOCAL) && !Inventory.ItemAttrsMatch(requirement, input[i]))
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.DISPLAY) && requirement.DisplayName != input[i].DisplayName)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.DESCRIPTION) && requirement.Description != input[i].Description)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.COLOR) && requirement.DrawColor.A != input[i].DrawColor.A && requirement.DrawColor.R != input[i].DrawColor.R
                     && requirement.DrawColor.G != input[i].DrawColor.G && requirement.DrawColor.B != input[i].DrawColor.B)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.DATUM) && requirement.Datum != input[i].Datum)
                {
                    thisone = false;
                }
                else if (mode.HasFlag(RecipeMode.BOUND) && requirement.IsBound != input[i].IsBound)
                {
                    thisone = false;
                }
                else if (thisone)
                {
                    used = i;
                    return true;
                }
            }
            used = -1;
            return false;
        }
    }

    public class RecipeResult
    {
        public ItemRecipe Recipe;
        public List<ItemStack> UsedInput;
    }

    public class ItemRecipe
    {
        public RecipeMode Mode;
        public CommandScript Determinant;
        public ItemStack[] Input;
        public Server TheServer;
        public int Index;
    }

    [Flags]
    public enum RecipeMode
    {
        TYPE = 1,
        SECONDARY = 2,
        TEXTURE = 4,
        MODEL = 8,
        SHARED = 16,
        LOCAL = 32,
        DISPLAY = 64,
        DESCRIPTION = 128,
        COLOR = 256,
        DATUM = 512,
        BOUND = 1024
    }
}
