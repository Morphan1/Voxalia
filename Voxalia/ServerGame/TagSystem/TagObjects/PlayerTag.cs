﻿using System;
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

        public PlayerEntity Internal;

        public PlayerTag(PlayerEntity p)
        {
            Internal = p;
        }

        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {

                // <--[tag]
                // @Base PlayerTag.name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the player's name.
                // @Example "Fortifier" .name returns "Fortifier".
                // -->
                case "name":
                    return new TextTag(Internal.Name).Handle(data.Shrink());
                // <--[tag]
                // @Base PlayerTag.health
                // @Group Status
                // @ReturnType NumberTag
                // @Returns the player's health.
                // @Example "Fortifier" .health could return "100".
                // -->
                case "health":
                    return new NumberTag(Internal.Health).Handle(data.Shrink());

                default:
                    return new EntityTag((Entity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.Name;
        }
    }
}