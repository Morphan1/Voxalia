using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class LocationTagBase : TemplateTagBase
    {
        // <--[tag]
        // @Base location[<LocationTag>]
        // @Group Mathematics
        // @ReturnType LocationTag
        // @Returns the location at the corresponding coordinates.
        // -->
        public LocationTagBase()
        {
            Name = "location";
        }

        public override string Handle(TagData data)
        {
            string lname = data.GetModifier(0);
            LocationTag ltag = LocationTag.For(lname);
            if (ltag == null)
            {
                return new TextTag("&{NULL}").Handle(data.Shrink());
            }
            return ltag.Handle(data.Shrink());
        }
    }
}
