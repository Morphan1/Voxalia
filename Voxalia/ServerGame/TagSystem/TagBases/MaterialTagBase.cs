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
using FreneticScript;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagBases
{
    class MaterialTagBase : TemplateTagBase
    {
        // <--[tagbase]
        // @Base material[<MaterialTag>]
        // @ReturnType MaterialTag
        // @Returns the material with the given material ID or name.
        // -->
        public MaterialTagBase()
        {
            Name = "material";
        }

        public override TemplateObject Handle(TagData data)
        {
            string input = data.GetModifier(0).ToLowerFast();
            try
            {
                Material mat = MaterialHelpers.FromNameOrNumber(input);
                return new MaterialTag(mat).Handle(data.Shrink());
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                data.Error("Invalid material '" + TagParser.Escape(input) + "'!");
                return new NullTag();
            }
        }
    }
}
