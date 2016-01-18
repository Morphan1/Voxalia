using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.ServerGame.TagSystem.TagObjects;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    class BlockItemEntityTag : TemplateObject
    {

        // <--[object]
        // @Type BlockItemEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any BlockItemEntity.
        // -->
        BlockItemEntity Internal;

        public BlockItemEntityTag(BlockItemEntity ent)
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
                // @Name BlockItemEntityTag.material
                // @Group General Information
                // @ReturnType MaterialTag
                // @Returns the BlockItemEntity's material.
                // @Example "2" .material could return "stone".
                // -->
                case "material":
                    return new MaterialTag((Material)Internal.Original.BlockMaterial).Handle(data.Shrink());
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
