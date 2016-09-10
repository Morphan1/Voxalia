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
    class PrimitiveEntityTag : TemplateObject
    {
        // <--[object]
        // @Type PrimitiveEntityTag
        // @SubType EntityTag
        // @Group Entities
        // @Description Represents any PrimitiveEntity.
        // -->
        PrimitiveEntity Internal;

        public PrimitiveEntityTag(PrimitiveEntity ent)
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
                // <--[tag]
                // @Name PrimitiveEntityTag.velocity
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the PrimitiveEntity's velocity as a LocationTag.
                // @Example "10" .velocity could return "(40, 10, 0)".
                // -->
                case "velocity":
                    return new LocationTag(Internal.GetVelocity(), null).Handle(data.Shrink());
                // <--[tag]
                // @Name PrimitiveEntityTag.scale
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the PrimitiveEntity's scale as a LocationTag.
                // @Example "5" .scale could return "(1, 1, 1)".
                // -->
                case "scale":
                    return new LocationTag(Internal.Scale, null).Handle(data.Shrink());

                default:
                    return new EntityTag((Entity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
