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
                // @Example "blocks/dirt" .item_type returns "block".
                // -->
                case "item_type":
                    return new TextTag(Internal.Name).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.item_type_secondary
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the name of the secondary type of this item, or null if none.
                // @Example "ammo/rifle" .item_type_secondary returns "rifle_gun".
                // @Example "blocks/dirt" .item_type_secondary returns null.
                // -->
                case "item_type_secondary":
                    return string.IsNullOrEmpty(Internal.SecondaryName) ? new NullTag() : new TextTag(Internal.SecondaryName).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.count
                // @Group General Information
                // @ReturnType IntegerTag
                // @Returns the number of this item held in this stack.
                // @Example "blocks/dirt" .count returns "1".
                // @Example "blocks/dirt" .with_count[5].count returns "5".
                // -->
                case "count":
                    return new IntegerTag(Internal.Count).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.datum
                // @Group General Information
                // @ReturnType IntegerTag
                // @Returns the "datum" number for this item.
                // @Example "blocks/dirt" .datum returns "3".
                // -->
                case "datum":
                    return new IntegerTag(Internal.Datum).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.draw_color
                // @Group General Information
                // @ReturnType ColorTag
                // @Returns the color that this item is to be drawn (rendered) with.
                // @Example "blocks/dirt" .draw_color returns "white".
                // @Example "blocks/dirt" .with_draw_color[red].draw_color returns "red".
                // -->
                case "draw_color":
                    return new ColorTag(Internal.DrawColor).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.description
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the description of the item.
                // @Example "blocks/dirt" .description returns "Dirty!".
                // -->
                case "description":
                    return new TextTag(Internal.Description).Handle(data.Shrink());
                // <--[tag]
                // @Name ItemTag.display_name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the display name of the item.
                // @Example "blocks/dirt" .display_name returns "Dirt".
                // -->
                case "display_name":
                    return new TextTag(Internal.DisplayName).Handle(data.Shrink());
                // TODO: All other item properties!
                // <--[tag]
                // @Name ItemTag.with_count[<IntegerTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified count of items. An input of 0 or less will result in an air item being returned.
                // @Example "blocks/dirt" .with_count[5] returns 5x "blocks/dirt".
                // -->
                case "with_count":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.Count = (int)IntegerTag.For(data, data.GetModifierObject(0)).Internal;
                        if (items.Count <= 0)
                        {
                            items = Internal.TheServer.Items.Air;
                        }
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_datum[<IntegerTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified item datum value.
                // @Example "blocks/dirt" .with_datum[1] returns a dirt block that is secretly made of stone.
                // -->
                case "with_datum":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.Datum = (int)IntegerTag.For(data, data.GetModifierObject(0)).Internal;
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_item_type[<TextTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified type name.
                // @Example "blocks/dirt" .with_item_type[rifle_gun] returns a rifle that looks like a block of dirt.
                // -->
                case "with_item_type":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.SetName(data.GetModifier(0));
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_secondary_type[<TextTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified secondary type name.
                // @Example "blocks/dirt" .with_item_type[bullet].with_secondary_type[rifle_gun] returns ammo for a rifle that looks like a block of dirt.
                // -->
                case "with_secondary_type":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.SecondaryName = data.GetModifier(0).ToLowerInvariant();
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_description[<TextTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified description.
                // @Example "blocks/dirt" .with_description[hello world] returns a rather friendly block of dirt.
                // -->
                case "with_description":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.Description = data.GetModifier(0);
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_display_name[<TextTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified display name.
                // @Example "blocks/dirt" .with_display_name[hello world] returns a rather friendly block of dirt.
                // -->
                case "with_display_name":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.DisplayName = data.GetModifier(0);
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // <--[tag]
                // @Name ItemTag.with_draw_color[<ColorTag>]
                // @Group Modification
                // @ReturnType ItemTag
                // @Returns a copy of this item with the specified draw_color.
                // @Example "blocks/dirt" .with_draw_color[red] returns "blocks/dirt" colored red.
                // -->
                case "with_draw_color":
                    {
                        ItemStack items = Internal.Duplicate();
                        items.DrawColor = ColorTag.For(data.GetModifierObject(0)).Internal;
                        return new ItemTag(items).Handle(data.Shrink());
                    }
                // TODO: Modifiers for all other item properties!
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
