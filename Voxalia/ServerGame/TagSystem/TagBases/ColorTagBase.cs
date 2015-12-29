using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class ColorTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base color[<ColorTag>]
        // @Group Mathematics
        // @ReturnType ItemTag
        // @Returns the color described by the given input.
        // -->
        public ColorTagBase()
        {
            Name = "color";
        }

        public override string Handle(TagData data)
        {
            string cname = data.GetModifier(0);
            ColorTag ctag = ColorTag.For(cname);
            if (ctag == null)
            {
                data.Error("Invalid color '" + TagParser.Escape(cname) + "'!");
                return "&{NULL}";
            }
            return ctag.Handle(data.Shrink());
        }
    }
}
