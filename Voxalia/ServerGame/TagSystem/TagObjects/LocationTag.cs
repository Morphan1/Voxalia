using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.Shared;
using Voxalia.ServerGame.OtherSystems;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.ServerGame.ServerMainSystem;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class LocationTag : TemplateObject
    {
        // <--[object]
        // @Type LocationTag
        // @SubType TextTag
        // @Group Worlds
        // @Description Represents any Location in the world.
        // -->
        public GameLocation Internal;

        public LocationTag(Location coord, World w)
        {
            Internal = new GameLocation(coord, w);
        }

        public LocationTag(GameLocation loc)
        {
            Internal = loc;
        }

        public static LocationTag For(Server tserver, TagData dat, string input)
        {
            string[] spl = input.Split(',');
            Location coord;
            if (spl.Length < 3)
            {
                dat.Error("Invalid LocationTag input!");
            }
            coord.X = NumberTag.For(dat, spl[0]).Internal;
            coord.Y = NumberTag.For(dat, spl[1]).Internal;
            coord.Z = NumberTag.For(dat, spl[2]).Internal;
            World w = null;
            if (spl.Length >= 4)
            {
                w = tserver.GetWorld(spl[3]);
                if (w == null)
                {
                    dat.Error("Invalid world for LocationTag input!");
                }
            }
            return new LocationTag(coord, w);
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
                // @Name LocationTag.x
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the X coordinate of this location.
                // @Example "0,1,2" .x returns "0".
                // -->
                case "x":
                    return new NumberTag(Internal.Coordinates.X).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.y
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the Y coordinate of this location.
                // @Example "0,1,2" .y returns "1".
                // -->
                case "y":
                    return new NumberTag(Internal.Coordinates.Y).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.z
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the Z coordinate of this location.
                // @Example "0,1,2" .z returns "2".
                // -->
                case "z":
                    return new NumberTag(Internal.Coordinates.Z).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.world
                // @Group General Information
                // @ReturnType WorldTag
                // @Returns the World of this location.
                // @Example "0,1,2,default" .world returns "default".
                // -->
                case "world":
                    return new WorldTag(Internal.World).Handle(data.Shrink());
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
