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
    class PhysicsEntityTag : TemplateObject
    {
        // <--[object]
        // @Type PhysicsEntityTag
        // @SubType EntityTag
        // @Group Entities
        // @Description Represents any PhysicsEntity.
        // -->
        PhysicsEntity Internal;

        public PhysicsEntityTag(PhysicsEntity ent)
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
                // @Name PhysicsEntityTag.mass
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the PhysicsEntity's mass.
                // @Example "10" .mass could return "40".
                // -->
                case "mass":
                    return new NumberTag(Internal.Mass).Handle(data.Shrink());
                // <--[tag]
                // @Name PhysicsEntityTag.bounciness
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the PhysicsEntity's bounciness (how much it bounces).
                // @Example "10" .bounciness could return "0.5".
                // -->
                case "bounciness":
                    return new NumberTag(Internal.GetBounciness()).Handle(data.Shrink());

                default:
                    return new EntityTag((Entity)Internal).Handle(data);
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
