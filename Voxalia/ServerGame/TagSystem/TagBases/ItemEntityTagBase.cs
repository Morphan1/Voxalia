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
    class ItemEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base item_entity[<ItemEntityTag>]
        // @Group Entities
        // @ReturnType ItemEntityTag
        // @Returns the item entity with the given entity ID.
        // -->
        Server TheServer;

        public ItemEntityTagBase(Server tserver)
        {
            Name = "item_entity";
            TheServer = tserver;
        }

        public override string Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerInvariant();
            if (long.TryParse(input, out eid))
            {
                foreach (Region r in TheServer.LoadedRegions)
                {
                    foreach (Entity e in r.Entities)
                    {
                        if (e.EID == eid && e is ItemEntity)
                        {
                            return new ItemEntityTag((ItemEntity)e).Handle(data.Shrink());
                        }
                    }
                }
            }
            data.Error("Invalid item entity '" + TagParser.Escape(input) + "'!");
            return "&{NULL}";
        }
    }
}
