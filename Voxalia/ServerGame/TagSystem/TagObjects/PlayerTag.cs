using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class PlayerTag : TemplateObject
    {
        // <--[object]
        // @Type PlayerTag
        // @SubType LivingEntityTag
        // @Group Entities
        // @Description Represents any PlayerEntity.
        // -->
        public PlayerEntity Internal;

        public PlayerTag(PlayerEntity p)
        {
            Internal = p;
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
                // @Name PlayerTag.name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the player's name.
                // @Example "Fortifier" .name returns "Fortifier".
                // -->
                case "name":
                    return new TextTag(Internal.Name).Handle(data.Shrink());
                // <--[tag]
                // @Name PlayerTag.health
                // @Group Status
                // @ReturnType NumberTag
                // @Returns the player's health.
                // @Example "Fortifier" .health could return "100".
                // -->
                case "health":
                    return new NumberTag(Internal.Health).Handle(data.Shrink());
                // <--[tag]
                // @Name PlayerTag.held_item
                // @Group Status
                // @ReturnType ItemTag
                // @Returns the item the player is currently holding.
                // @Example "mcmonkey" .held_item could return "bullet".
                // -->
                case "held_item":
                    return new ItemTag(Internal.Items.GetItemForSlot(Internal.Items.cItem)).Handle(data.Shrink());
                default:
                    return new LivingEntityTag((LivingEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.Name;
        }
    }
}
