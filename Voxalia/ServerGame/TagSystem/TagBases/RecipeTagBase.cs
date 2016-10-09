//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
            RecipeTag rtag = RecipeTag.For(TheServer, data, rdata);
            if (rtag == null)
            {
                data.Error("Invalid recipe '" + TagParser.Escape(rdata.ToString()) + "'!");
                return new NullTag();
            }
            return rtag.Handle(data.Shrink());
        }
    }
}
