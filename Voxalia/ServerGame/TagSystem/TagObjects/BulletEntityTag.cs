using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class BulletEntityTag : TemplateObject
    {
        // <--[object]
        // @Type BulletEntityTag
        // @SubType PrimitiveEntityTag
        // @Group Entities
        // @Description Represents any BulletEntity.
        // -->
        BulletEntity Internal;

        public BulletEntityTag(BulletEntity ent)
        {
            Internal = ent;
        }

        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // TODO: Tags
                default:
                    return new PrimitiveEntityTag((PrimitiveEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
