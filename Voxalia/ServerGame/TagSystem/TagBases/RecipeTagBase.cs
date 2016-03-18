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
    class RecipeTagBase : TemplateTagBase
    {
        public Server TheServer;

        // <--[tagbase]
        // @Base recipe[<RecipeTag>]
        // @ReturnType RecipeTag
        // @Returns the recipe described by the given input.
        // -->
        public RecipeTagBase(Server tserver)
        {
            TheServer = tserver;
            Name = "recipe";
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject rdata = data.GetModifierObject(0);
            RecipeTag rtag = RecipeTag.For(TheServer, rdata);
            if (rtag == null)
            {
                data.Error("Invalid recipe '" + TagParser.Escape(rdata.ToString()) + "'!");
                return new NullTag();
            }
            return rtag.Handle(data.Shrink());
        }
    }
}
