using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
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
            string input = data.GetModifier(0).ToLowerInvariant();
            try
            {
                Material mat = MaterialHelpers.FromNameOrNumber(input);
                return new MaterialTag(mat).Handle(data.Shrink());
            }
            catch (Exception)
            {
                data.Error("Invalid material '" + TagParser.Escape(input) + "'!");
                return new TextTag("&{NULL}");
            }
        }
    }
}
