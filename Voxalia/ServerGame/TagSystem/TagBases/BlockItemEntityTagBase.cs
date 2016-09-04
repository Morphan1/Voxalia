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
    class BlockItemEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base block_item_entity[<BlockItemEntityTag>]
        // @Group Entities
        // @ReturnType BlockItemEntityTag
        // @Returns the block item entity with the given entity ID.
        // -->
        Server TheServer;

        public BlockItemEntityTagBase(Server tserver)
        {
            Name = "block_item_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                Entity e = TheServer.GetEntity(eid);
                if (e != null && e is BlockItemEntity)
                {
                    return new BlockItemEntityTag((BlockItemEntity)e).Handle(data.Shrink());
                }
            }
            data.Error("Invalid block item entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
