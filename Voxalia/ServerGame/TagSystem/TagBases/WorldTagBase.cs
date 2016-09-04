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
            string rname = data.GetModifier(0).ToLowerFast();
            foreach (World w in TheServer.LoadedWorlds)
            {
                if (w.Name.ToLowerFast() == rname)
                {
                    return new WorldTag(w).Handle(data.Shrink());
                }
            }
            data.Error("Invalid world '" + TagParser.Escape(rname) + "'!");
            return new NullTag();
        }
    }
}
