using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
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

        public override string Handle(TagData data)
        {
            string input = data.GetModifier(0).ToLower();
            try {
                Material mat = MaterialHelpers.FromNameOrNumber(input);
                return new MaterialTag(mat).Handle(data.Shrink());
            }
            catch (Exception)
            {
                return new TextTag("${NULL}").Handle(data.Shrink());
            }
        }
    }
}
