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
    class RecipeResultTagBase : TemplateTagBase
    {
        public Server TheServer;

        // <--[tagbase]
        // @Base recipe_result[<RecipeTag>]
        // @ReturnType RecipeResultTag
        // @Returns the recipe result described by the given input.
        // -->
        public RecipeResultTagBase(Server tserver)
        {
            TheServer = tserver;
            Name = "recipe_result";
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject rdata = data.GetModifierObject(0);
            RecipeResultTag rtag = RecipeResultTag.For(TheServer, data, rdata);
            if (rtag == null)
            {
                data.Error("Invalid recipe result '" + TagParser.Escape(rdata.ToString()) + "'!");
                return new NullTag();
            }
            return rtag.Handle(data.Shrink());
        }
    }
}
