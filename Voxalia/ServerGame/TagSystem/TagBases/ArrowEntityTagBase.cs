using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using FreneticScript;

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

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                Entity e = TheServer.GetEntity(eid);
                if (e != null && e is ArrowEntity)
                {
                    return new ArrowEntityTag((ArrowEntity)e).Handle(data.Shrink());
                }
            }
            data.Error("Invalid arrow entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
