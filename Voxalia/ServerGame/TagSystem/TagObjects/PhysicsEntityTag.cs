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
    class PhysicsEntityTag : TemplateObject
    {
        // <--[object]
        // @Type PhysicsEntityTag
        // @SubType EntityTag
        // @Group Entities
        // @Description Represents any PhysicsEntity.
        // -->
        PhysicsEntity Internal;

        public PhysicsEntityTag(PhysicsEntity ent)
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
                    return new EntityTag((Entity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            if (Internal is PlayerEntity)
            {
                return ((PlayerEntity)Internal).Name;
            }
            else
            {
                return Internal.EID.ToString();
            }
        }
    }
}
