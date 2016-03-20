using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
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

        public static PlayerTag For(Server tserver, string pname)
        {
            long pid;
            if (long.TryParse(pname, out pid))
            {
                foreach (PlayerEntity player in tserver.Players)
                {
                    if (player.EID == pid)
                    {
                        return new PlayerTag(player);
                    }
                }
            }
            else
            {
                pname = pname.ToLowerInvariant();
                foreach (PlayerEntity player in tserver.Players)
                {
                    if (player.Name.ToLowerInvariant() == pname)
                    {
                        return new PlayerTag(player);
                    }
                }
            }
            return null;
        }

        public static PlayerTag For(Server tserver, TemplateObject obj)
        {
            return (obj is PlayerTag) ? (PlayerTag)obj : For(tserver, obj.ToString());
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
                // @Name PlayerTag.is_afk
                // @Group Status
                // @ReturnType BooleanTag
                // @Returns whether the player is AFK (Away From Keyboard) currently.
                // @Other this is detected as "no input whatsoever" from the player. Any number of things could potentially knock a player out of "AFK" status.
                // -->
                case "is_afk":
                    return new BooleanTag(Internal.IsAFK).Handle(data.Shrink());
                // <--[tag]
                // @Name PlayerTag.afk_time
                // @Group Status
                // @ReturnType IntegerTag
                // @Returns the amount of time the player has been AFK (Away From Keyboard) currently. This is a number in seconds.
                // @Other this is detected as "no input whatsoever" from the player. Any number of things could potentially knock a player out of "AFK" status.
                // @Example "mcmonkey" .afk_time could return "5".
                // -->
                case "afk_time":
                    return new IntegerTag(Internal.TimeAFK).Handle(data.Shrink());
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
