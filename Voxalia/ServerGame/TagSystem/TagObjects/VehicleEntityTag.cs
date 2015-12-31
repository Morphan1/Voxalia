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
    class VehicleEntityTag : TemplateObject
    {
        // <--[object]
        // @Type VehicleEntityTag
        // @SubType ModelEntityTag
        // @Group Entities
        // @Description Represents any VehicleEntity.
        // -->
        VehicleEntity Internal;

        public VehicleEntityTag(VehicleEntity ent)
        {
            Internal = ent;
        }

        public override TemplateObject Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return this;
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Name VehicleEntityTag.has_wheels
                // @Group General Information
                // @ReturnType BooleanTag
                // @Returns whether the VehicleEntity has wheels.
                // @Example "5" .has_wheels could return "true".
                // -->
                case "has_wheels":
                    return new BooleanTag(Internal.hasWheels).Handle(data.Shrink());

                default:
                    return new ModelEntityTag((ModelEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
