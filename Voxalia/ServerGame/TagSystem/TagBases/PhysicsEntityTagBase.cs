using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.ServerMainSystem;
using Voxalia.ServerGame.EntitySystem;
using FreneticScript;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class PhysicsEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base physics_entity[<PhysicsEntityTag>]
        // @Group Entities
        // @ReturnType PhysicsEntityTag
        // @Returns the physics entity with the given entity ID or name.
        // -->
        Server TheServer;

        public PhysicsEntityTagBase(Server tserver)
        {
            Name = "physics_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                foreach (Region r in TheServer.LoadedRegions)
                {
                    foreach (Entity e in r.Entities)
                    {
                        if (e.EID == eid && e is PhysicsEntity)
                        {
                            return new PhysicsEntityTag((PhysicsEntity)e).Handle(data.Shrink());
                        }
                    }
                }
            }
            else
            {
                foreach (PlayerEntity p in TheServer.Players)
                {
                    if (p.Name.ToLowerFast() == input)
                    {
                        return new PhysicsEntityTag((PhysicsEntity)p).Handle(data.Shrink());
                    }
                }
            }
            data.Error("Invalid physics entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
