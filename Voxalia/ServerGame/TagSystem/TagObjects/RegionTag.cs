using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class RegionTag : TemplateObject
    {
        // <--[object]
        // @Type RegionTag
        // @SubType TextTag
        // @Group Entities
        // @Description Represents any Region.
        // -->
        Region Internal;

        public RegionTag(Region reg)
        {
            Internal = reg;
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
                // @Name RegionTag.name
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the region's name.
                // @Example "default" .name returns "default".
                // -->
                case "name":
                    return new TextTag(Internal.Name).Handle(data.Shrink());

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
