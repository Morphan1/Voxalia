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
    public class RecipeResultTag : TemplateObject
    {
        // <--[object]
        // @Type RecipeResultTag
        // @SubType TextTag
        // @Description Represents any item recipe.
        // -->
        public RecipeResult Internal;
        
        public static RecipeResultTag For(Server tserver, TagData data, TemplateObject input)
        {
            if (input is RecipeResultTag)
            {
                return (RecipeResultTag)input;
            }
            ListTag list = ListTag.For(input);
            RecipeTag recipe = RecipeTag.For(tserver, data, list.ListEntries[0]);
            List<ItemStack> used = new List<ItemStack>();
            for (int i = 1; i < list.ListEntries.Count; i++)
            {
                used.Add(ItemTag.For(tserver, list.ListEntries[i]).Internal);
            }
            return new RecipeResultTag(new RecipeResult() { Recipe = recipe.Internal, UsedInput = used });
        }

        public RecipeResultTag(RecipeResult reciperes)
        {
            Internal = reciperes;
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
                // @Name RecipeResultTag.recipe
                // @Group General Information
                // @ReturnType RecipeTag
                // @Returns the original recipe used to form this recipe result.
                // -->
                case "recipe":
                    return new RecipeTag(Internal.Recipe).Handle(data.Shrink());
                // <--[tag]
                // @Name RecipeResultTag.used_input
                // @Group General Information
                // @ReturnType ListTag
                // @Returns the input items used by this recipe result.
                // @Example "1|blocks/grass|blocks/dirt" .result returns "blocks/grass|blocks/dirt|".
                // -->
                case "used_input":
                    {
                        ListTag list = new ListTag();
                        for (int i = 0; i < Internal.UsedInput.Count; i++)
                        {
                            list.ListEntries.Add(new ItemTag(Internal.UsedInput[i]));
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
            list.ListEntries.Add(new IntegerTag(Internal.Recipe.Index));
            foreach (ItemStack item in Internal.UsedInput)
            {
                list.ListEntries.Add(new ItemTag(item));
            }
            return list.ToString();
        }
    }
}
