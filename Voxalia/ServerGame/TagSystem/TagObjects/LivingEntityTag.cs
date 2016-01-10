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
    class LivingEntityTag : TemplateObject
    {
        // <--[object]
        // @Type LivingEntityTag
        // @SubType PhysicsEntityTag
        // @Group Entities
        // @Description Represents any LivingEntity.
        // -->
        LivingEntity Internal;

        public LivingEntityTag(LivingEntity ent)
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
                // @Name LivingEntityTag.health
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's health.
                // @Example "5" .health could return "100".
                // -->
                case "health":
                    return new NumberTag(Internal.Health).Handle(data.Shrink());
                // <--[tag]
                // @Name LivingEntityTag.max_health
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's maximum health.
                // @Example "5" .max_health could return "100".
                // -->
                case "max_health":
                    return new NumberTag(Internal.MaxHealth).Handle(data.Shrink());
                // <--[tag]
                // @Name LivingEntityTag.health_percentage
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the LivingEntity's health as a percentage of its maximum health.
                // @Example "5" .health_percentage could return "70" - indicating 70%.
                // -->
                case "health_percentage":
                    return new NumberTag((Internal.MaxHealth / Internal.Health) * 100).Handle(data.Shrink());

                default:
                    return new PhysicsEntityTag((PhysicsEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            if (Internal is PlayerEntity)
            {
                return ((PlayerEntity)Internal).Name;
            }
            else
            {
                return Internal.EID.ToString();
            }
        }
    }
}
