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
    class PlayerTag : TemplateTags
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
                // @Base PlayerTag.name
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the player's location.
                // @Example "Fortifier" .location could return "(1, 1, 1)".
                // -->
                case "location":
                    return new LocationTag(Internal.GetPosition()).Handle(data.Shrink());
                // <--[tag]
                // @Base PlayerTag.health
                // @Group Status
                // @ReturnType TextTag
                // @Returns the player's health.
                // @Example "Fortifier" .health could return "100".
                // -->
                case "health":
                    return new TextTag(Internal.Health.ToString()).Handle(data.Shrink());

                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.Name;
        }
    }
}
