using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class RegionTag : TemplateObject
    {

        Region Internal;

        public RegionTag(Region r)
        {
            Internal = r;
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
                // @Base RegionTag.name
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
