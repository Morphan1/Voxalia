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
    class EntityTagBase : TemplateTagBase
    {

        public Entity Internal;
        Server TheServer;

        // <--[tag]
        // @Base entity[<EntityTag>]
        // @Group Entities
        // @ReturnType EntityTag
        // @Returns the entity with the given ID or name.
        // -->
        public EntityTagBase(Server tserver)
        {
            Name = "entity";
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
                        if (e.EID == eid)
                        {
                            return new EntityTag(e).Handle(data.Shrink());
                        }
                    }
                }
            }
            else
            {
                foreach (PlayerEntity p in TheServer.Players)
                {
                    if (p.Name.ToLower() == input)
                    {
                        return new EntityTag((Entity) p).Handle(data.Shrink());
                    }
                }
            }
            return new TextTag("&{NULL}").Handle(data.Shrink());
        }
    }
}
