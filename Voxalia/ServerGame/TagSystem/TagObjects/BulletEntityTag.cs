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
    class BulletEntityTag : TemplateObject
    {
        // <--[object]
        // @Type BulletEntityTag
        // @SubType PrimitiveEntityTag
        // @Group Entities
        // @Description Represents any BulletEntity.
        // -->
        BulletEntity Internal;

        public BulletEntityTag(BulletEntity ent)
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
                // @Name BulletEntityTag.damage
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the amount of damage the BulletEntity will do, not counting splash or other special damage.
                // @Example "10" .damage could return "1".
                // -->
                case "damage":
                    return new NumberTag(Internal.Damage).Handle(data.Shrink());
                // <--[tag]
                // @Name BulletEntityTag.splash_size
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the radius of the BulletEntity's splash damage area in blocks.
                // @Example "10" .splash_size could return "0".
                // -->
                case "splash_size":
                    return new NumberTag(Internal.SplashSize).Handle(data.Shrink());

                default:
                    return new PrimitiveEntityTag((PrimitiveEntity)Internal).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.EID.ToString();
        }
    }
}
