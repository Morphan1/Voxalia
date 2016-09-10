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
    class EntityTag : TemplateObject
    {
        // <--[object]
        // @Type EntityTag
        // @SubType TextTag
        // @Group Entities
        // @Description Represents any Entity.
        // -->
        Entity Internal;

        public EntityTag(Entity ent)
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
                // @Name EntityTag.location
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the entity's location.
                // @Example "2" .location could return "(1, 1, 1)".
                // -->
                case "location":
                    return new LocationTag(Internal.GetPosition(), Internal.TheRegion.TheWorld).Handle(data.Shrink());
                // <--[tag]
                // @Name EntityTag.eid
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the entity's Entity Identification number (EID).
                // @Example "2" .eid returns "2".
                // -->
                case "eid":
                    return new NumberTag(Internal.EID).Handle(data.Shrink());
                // <--[tag]
                // @Name EntityTag.entity_type
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the entity's entity type.
                // @Example "2" .entity_type could return "BulletEntity".
                // -->
                case "entity_type":
                    return new TextTag(Internal.GetEntityType().ToString()).Handle(data.Shrink());

                default:
                    return new TextTag(ToString()).Handle(data);
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
