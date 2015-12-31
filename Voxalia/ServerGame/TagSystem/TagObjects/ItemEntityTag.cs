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
    class ItemEntityTag : TemplateObject
    {
        // <--[object]
        // @Type ItemEntityTag
        // @SubType ModelEntity
        // @Group Entities
        // @Description Represents any ItemEntity.
        // -->
        ItemEntity Internal;

        public ItemEntityTag(ItemEntity ent)
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
                // TODO: Tags
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
