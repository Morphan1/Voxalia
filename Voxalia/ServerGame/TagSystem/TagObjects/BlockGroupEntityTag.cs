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
    class BlockGroupEntityTag : TemplateObject
    {

        // <--[object]
        // @Type BlockGroupEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any BlockGroupEntity.
        // -->
        BlockGroupEntity Internal;

        public BlockGroupEntityTag(BlockGroupEntity ent)
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
                // @Name BlockGroupEntityTag.width_x
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the width of the BlockGroupEntity along its X axis.
                // @Example "10" .width_x could return "1".
                // -->
                case "width_x":
                    return new NumberTag(Internal.XWidth).Handle(data.Shrink());
                // <--[tag]
                // @Name BlockGroupEntityTag.width_y
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the width of the BlockGroupEntity along its Y axis.
                // @Example "10" .width_y could return "1".
                // -->
                case "width_y":
                    return new NumberTag(Internal.YWidth).Handle(data.Shrink());
                // <--[tag]
                // @Name BlockGroupEntityTag.width_y
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the width of the BlockGroupEntity along its Z axis.
                // @Example "10" .width_z could return "1".
                // -->
                case "width_z":
                    return new NumberTag(Internal.ZWidth).Handle(data.Shrink());

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
