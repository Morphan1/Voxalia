using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class ArrowEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base arrow_entity[<ArrowEntityTag>]
        // @Group Entities
        // @ReturnType ArrowEntityTag
        // @Returns the arrow entity with the given entity ID.
        // -->
        Server TheServer;

        public ArrowEntityTagBase(Server tserver)
        {
            Name = "arrow_entity";
            TheServer = tserver;
        }

        public override string Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLower();
            if (long.TryParse(input, out eid))
            {
                foreach (Region r in TheServer.LoadedRegions)
                {
                    foreach (Entity e in r.Entities)
                    {
                        if (e.EID == eid && e is ArrowEntity)
                        {
                            return new ArrowEntityTag((ArrowEntity)e).Handle(data.Shrink());
                        }
                    }
                }
            }
            return new TextTag("&{NULL}").Handle(data.Shrink());
        }
    }
}
