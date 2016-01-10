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
    class GlowstickEntityTag : TemplateObject
    {
        // <--[object]
        // @Type GlowstickEntityTag
        // @SubType GrenadeEntityTag
        // @Group Entities
        // @Description Represents any GlowstickEntity.
        // -->
        GlowstickEntity Internal;

        public GlowstickEntityTag(GlowstickEntity ent)
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
                    return new GrenadeEntityTag((GrenadeEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
