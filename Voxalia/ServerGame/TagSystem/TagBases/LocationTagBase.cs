using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class LocationTagBase : TemplateTagBase
    {
        Server TheServer;

        // <--[tagbase]
        // @Base location[<LocationTag>]
        // @Group Mathematics
        // @ReturnType LocationTag
        // @Returns the location at the corresponding coordinates.
        // -->
        public LocationTagBase(Server tserver)
        {
            TheServer = tserver;
            Name = "location";
        }

        public override TemplateObject Handle(TagData data)
        {
            string lname = data.GetModifier(0);
            LocationTag ltag = LocationTag.For(TheServer, data, lname);
            if (ltag == null)
            {
                data.Error("Invalid location '" + TagParser.Escape(lname) + "'!");
                return new NullTag();
            }
            return ltag.Handle(data.Shrink());
        }
    }
}
