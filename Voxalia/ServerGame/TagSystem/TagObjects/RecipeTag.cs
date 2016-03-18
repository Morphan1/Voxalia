using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.ItemSystem;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class RecipeTag : TemplateObject
    {
        // <--[object]
        // @Type RecipeTag
        // @SubType TextTag
        // @Description Represents any item recipe.
        // -->
        public ItemRecipe Internal;

        public static RecipeTag For(Server tserver, string input)
        {
            ListTag list = ListTag.For(input);
            ItemRecipe recip = new ItemRecipe();
            recip.TheServer = tserver;
            // TODO: handle errors neatly
            recip.Result = ItemTag.For(tserver, list.ListEntries[0]).Internal;
            List<ItemStack> items = new List<ItemStack>();
            for (int i = 1; i < list.ListEntries.Count; i++)
            {
                items.Add(ItemTag.For(tserver, list.ListEntries[i]).Internal);
            }
            recip.Input = items.ToArray();
            return new RecipeTag(recip);
        }
        
        public static RecipeTag For(Server tserver, TemplateObject input)
        {
            return input is RecipeTag ? (RecipeTag)input : For(tserver, input);
        }

        public RecipeTag(ItemRecipe recipe)
        {
            Internal = recipe;
        }

        public override TemplateObject Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name ItemTag.result
                // @Group General Information
                // @ReturnType ItemTag
                // @Returns the result of this recipe.
                // @Example "blocks/grass|blocks/dirt" .result returns "blocks/grass".
                // -->
                case "result":
                    return new ItemTag(Internal.Result).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.input
                // @Group General Information
                // @ReturnType ListTag
                // @Returns the result of this recipe.
                // @Example "blocks/grass|blocks/dirt" .result returns "blocks/dirt|".
                // -->
                case "input":
                    {
                        ListTag list = new ListTag();
                        for (int i = 0; i < Internal.Input.Length; i++)
                        {
                            list.ListEntries.Add(new ItemTag(Internal.Input[i]));
                        }
                        return list.Handle(data.Shrink());
                    }
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            ListTag list = new ListTag();
            list.ListEntries.Add(new ItemTag(Internal.Result));
            for (int i = 0; i < Internal.Input.Length; i++)
            {
                list.ListEntries.Add(new ItemTag(Internal.Input[i]));
            }
            return list.ToString();
        }
    }
}
