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
    class ItemTagBase : TemplateTagBase
    {
        public Server TheServer;

        // <--[tagbase]
        // @Base item[<ItemTag>]
        // @ReturnType ItemTag
        // @Returns the item described by the given input.
        // -->
        public ItemTagBase(Server tserver)
        {
            TheServer = tserver;
            Name = "item";
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject iname = data.GetModifierObject(0);
            ItemTag itag = ItemTag.For(TheServer, iname);
            if (itag == null)
            {
                data.Error("Invalid item '" + TagParser.Escape(iname.ToString()) + "'!");
                return new NullTag();
            }
            return itag.Handle(data.Shrink());
        }
    }
}
