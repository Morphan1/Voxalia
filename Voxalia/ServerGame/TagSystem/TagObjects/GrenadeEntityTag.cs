using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class GrenadeEntityTag : TemplateObject
    {
        // <--[object]
        // @Type GrenadeEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any GrenadeEntity.
        // -->
        GrenadeEntity Internal;

        public GrenadeEntityTag(GrenadeEntity ent)
        {
            Internal = ent;
        }

        public override TemplateObject Handle(TagData data)
        {
            if (data.Remaining == 0)
            {
                return this;
            }
            switch (data[0])
            {
                // TODO: Tags
                default:
                    return new PhysicsEntityTag((PhysicsEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
