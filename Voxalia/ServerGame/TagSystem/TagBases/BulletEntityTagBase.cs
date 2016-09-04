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
using FreneticScript;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class BulletEntityTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base bullet_entity[<BulletEntityTag>]
        // @Group Entities
        // @ReturnType BulletEntityTag
        // @Returns the bullet entity with the given entity ID.
        // -->
        Server TheServer;

        public BulletEntityTagBase(Server tserver)
        {
            Name = "bullet_entity";
            TheServer = tserver;
        }

        public override TemplateObject Handle(TagData data)
        {
            long eid;
            string input = data.GetModifier(0).ToLowerFast();
            if (long.TryParse(input, out eid))
            {
                Entity e = TheServer.GetEntity(eid);
                if (e != null && e is BulletEntity)
                {
                    return new BulletEntityTag((BulletEntity)e).Handle(data.Shrink());
                }
            }
            data.Error("Invalid bullet entity '" + TagParser.Escape(input) + "'!");
            return new NullTag();
        }
    }
}
