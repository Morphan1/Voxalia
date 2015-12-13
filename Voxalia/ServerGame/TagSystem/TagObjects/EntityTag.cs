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
    class EntityTag : TemplateObject
    {

        Entity Internal;

        public EntityTag(Entity ent)
        {
            Internal = ent;
        }

        public override string Handle(TagData data)
        {
            if (data.Input.Count == 0)
            {
                return ToString();
            }
            switch (data.Input[0])
            {
                // <--[tag]
                // @Base EntityTag.location
                // @Group General Information
                // @ReturnType LocationTag
                // @Returns the entity's location.
                // @Example "2" .location could return "(1, 1, 1)".
                // -->
                case "location":
                    return new LocationTag(Internal.GetPosition()).Handle(data.Shrink());
                // <--[tag]
                // @Base EntityTag.eid
                // @Group General Information
                // @ReturnType TextTag
                // @Returns the entity's Entity Identification number (EID).
                // @Example "2" .eid returns "2".
                // -->
                case "eid":
                    return new TextTag(Internal.EID).Handle(data.Shrink());

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
