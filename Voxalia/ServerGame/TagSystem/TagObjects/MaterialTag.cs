using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class MaterialTag : TemplateObject
    {
        // <--[object]
        // @Type MaterialTag
        // @SubType NONE
        // @Group To Be Decided.
        // @Description Represents any material.
        // -->
        Material Internal;

        public MaterialTag(Material m)
        {
            Internal = m;
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
                // @Name MaterialTag.name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the material's name.
                // @Example "stone" .name returns "stone".
                // -->
                case "name":
                    return new TextTag(ToString()).Handle(data.Shrink());
                // <--[tag]
                // @Name MaterialTag.speed_mod
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the material's speed modification.
                // @Example "stone" .speed_mod returns "1.1".
                // -->
                case "speed_mod":
                    return new NumberTag(Internal.GetSpeedMod()).Handle(data.Shrink());
                // TODO: More tags
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.GetName();
        }
    }
}
