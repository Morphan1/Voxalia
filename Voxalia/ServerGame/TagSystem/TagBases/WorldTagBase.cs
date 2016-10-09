//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class WorldTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base world[<WorldTag>]
        // @Group World
        // @ReturnType RegionTag
        // @Returns the region with the given name.
        // -->
        Server TheServer;

        public WorldTagBase(Server tserver)
        {
            Name = "world";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            string rname = data.GetModifier(0);
            World w = TheServer.GetWorld(rname);
            if (w != null)
            {
                return new WorldTag(w);
            }
            data.Error("Invalid world '" + TagParser.Escape(rname) + "'!");
            return new NullTag();
        }
    }
}
