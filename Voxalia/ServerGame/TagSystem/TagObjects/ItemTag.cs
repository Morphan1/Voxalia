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
    class ItemTag : TemplateObject
    {
        // <--[object]
        // @Type ItemTag
        // @SubType TextTag
        // @Description Represents any Item.
        // -->
        public ItemStack Internal;

        public ItemTag(ItemStack itm)
        {
            Internal = itm;
        }

        public static ItemTag For(Server tserver, string input)
        {
            if (input.Contains('['))
            {
                return new ItemTag(ItemStack.FromString(tserver, input));
            }
            else
            {
                ItemStack its = tserver.Items.GetItem(input);
                if (its == null)
                {
                    return null;
                }
                return new ItemTag(its);
            }
        }

        public static ItemTag For(Server tserver, TemplateObject input)
        {
            return input is ItemTag ? (ItemTag)input : For(tserver, input.ToString());
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
                // @Name ItemTag.item_type
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the name of the type of this item.
                // @Example "bullet" .item_type returns "bullet".
                // -->
                case "item_type":
                    return new TextTag(Internal.Name).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.ToEscapedString();
        }
    }
}
