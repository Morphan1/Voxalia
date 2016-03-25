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
    class ModelEntityTag : TemplateObject
    {
        // <--[object]
        // @Type ModelEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any ModelEntity.
        // -->
        ModelEntity Internal;

        public ModelEntityTag(ModelEntity ent)
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
                // @Name ModelEntityTag.model
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the ModelEntity's model.
                // @Example "5" .model could return "cube".
                // -->
                case "model":
                    return new TextTag(Internal.model).Handle(data.Shrink());
                // <--[tag]
                // @Name ModelEntityTag.scale
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the ModelEntity's scale as a LocationTag.
                // @Example "5" .scale could return "(1, 1, 1)".
                // -->
                case "scale":
                    return new LocationTag(Internal.scale).Handle(data.Shrink());

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
