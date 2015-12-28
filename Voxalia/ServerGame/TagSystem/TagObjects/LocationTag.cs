using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frenetic.TagHandlers;
using Frenetic.TagHandlers.Objects;
using Voxalia.Shared;


namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class LocationTag : TemplateObject
    {
        public Location Internal;

        public LocationTag(Location loc)
        {
            Internal = loc;
        }

        public static LocationTag For(string input)
        {
            return new LocationTag(Location.FromString(input));
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
                // @Name LocationTag.x
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the X coordinate of this location.
                // @Example "0,1,2" .x returns "0".
                // -->
                case "x":
                    return new NumberTag(Internal.X).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.y
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the Y coordinate of this location.
                // @Example "0,1,2" .y returns "1".
                // -->
                case "y":
                    return new NumberTag(Internal.Y).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.z
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the Z coordinate of this location.
                // @Example "0,1,2" .z returns "2".
                // -->
                case "z":
                    return new NumberTag(Internal.Z).Handle(data.Shrink());

                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return Internal.ToString();
        }

    }
}
