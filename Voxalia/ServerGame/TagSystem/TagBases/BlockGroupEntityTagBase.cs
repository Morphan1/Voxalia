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

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class BlockGroupEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base block_group_entity[<BlockGroupEntityTag>]
        // @Group Entities
        // @ReturnType BlockGroupEntityTag
        // @Returns the block group entity with the given entity ID.
        // -->
        Server TheServer;

        public BlockGroupEntityTagBase(Server tserver)
        {
            Name = "block_group_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerInvariant();
            if (long.TryParse(input, out eid))
            {
                foreach (Region r in TheServer.LoadedRegions)
                {
                    foreach (Entity e in r.Entities)
                    {
                        if (e.EID == eid && e is BlockGroupEntity)
                        {
                            return new BlockGroupEntityTag((BlockGroupEntity)e).Handle(data.Shrink());
                        }
                    }
                }
            }
            data.Error("Invalid block group entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
