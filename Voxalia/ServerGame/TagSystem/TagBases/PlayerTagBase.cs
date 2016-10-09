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
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class PlayerTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base player[<PlayerTag>]
        // @Group Entities
        // @ReturnType PlayerTag
        // @Returns the player with the given name or entity ID.
        // -->
        Server TheServer;

        public PlayerTagBase(Server tserver)
        {
            Name = "player";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            TemplateObject pname = data.GetModifierObject(0);
            ItemTag ptag = ItemTag.For(TheServer, pname);
            if (ptag == null)
            {
                data.Error("Invalid player '" + TagParser.Escape(pname.ToString()) + "'!");
                return new NullTag();
            }
            return ptag.Handle(data.Shrink());
        }
    }
}
