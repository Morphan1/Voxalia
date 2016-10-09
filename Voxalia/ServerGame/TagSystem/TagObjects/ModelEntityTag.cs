//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
                    return new LocationTag(Internal.scale, null).Handle(data.Shrink());

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
