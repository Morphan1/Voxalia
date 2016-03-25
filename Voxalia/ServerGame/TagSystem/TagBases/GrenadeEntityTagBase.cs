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
    class GrenadeEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base grenade_entity[<GrenadeEntityTag>]
        // @Group Entities
        // @ReturnType GrenadeEntityTag
        // @Returns the grenade entity with the given entity ID.
        // -->
        Server TheServer;

        public GrenadeEntityTagBase(Server tserver)
        {
            Name = "grenade_entity";
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
                        if (e.EID == eid && e is GrenadeEntity)
                        {
                            return new GrenadeEntityTag((GrenadeEntity)e).Handle(data.Shrink());
                        }
                    }
                }
            }
            data.Error("Invalid grenade entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
