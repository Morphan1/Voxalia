using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FreneticScript.TagHandlers;
using FreneticScript.TagHandlers.Objects;
using Voxalia.Shared;
using FreneticScript;

namespace Voxalia.ServerGame.TagSystem.TagObjects
{
    public class ColorTag : TemplateObject
    {
        // <--[object]
        // @Type ColorTag
        // @SubType TextTag
        // @Group Mathematics
        // @Description Represents any color.
        // -->
        public System.Drawing.Color Internal;

        public ColorTag(System.Drawing.Color loc)
        {
            Internal = loc;
        }

        static bool TryParseComponent(string inp, out float comp)
        {
            float t;
            if (float.TryParse(inp, out t))
            {
                if (t >= 0 && t <= 1)
                {
                    comp = t;
                    return true;
                }
            }
            comp = 0;
            return false;
        }

        public static ColorTag For(string input)
        {
            string[] split = input.SplitFast(',');
            if (split.Length == 1)
            {
                byte b;
                if (byte.TryParse(input, out b))
                {
                    return new ColorTag(Colors.ForByte(b));
                }
            }
            else if (split.Length == 3)
            {
                float r, g, b;
                if (TryParseComponent(split[0], out r) && TryParseComponent(split[1], out g) && TryParseComponent(split[2], out b))
                {
                    return new ColorTag(System.Drawing.Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255)));
                }
            }
            else if (split.Length == 4)
            {
                float r, g, b, a;
                if (TryParseComponent(split[0], out r) && TryParseComponent(split[1], out g) && TryParseComponent(split[2], out b) && TryParseComponent(split[3], out a))
                {
                    return new ColorTag(System.Drawing.Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255)));
                }
            }
            return new ColorTag(Colors.ForByte(Colors.ForName(input.ToUpperInvariant())));
        }

        public static ColorTag For(TemplateObject obj)
        {
            return obj is ColorTag ? (ColorTag)obj : For(obj.ToString());
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
                // @Name LocationTag.red
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the red component of this color.
                // @Example "0,0.33,0.66,1" .r returns "0".
                // -->1
                case "red":
                    return new NumberTag(Internal.R / 255f).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.green
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the green component of this color.
                // @Example "0,0.33,0.66,1" .g returns "0.33".
                // -->1
                case "green":
                    return new NumberTag(Internal.G / 255f).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.blue
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the blue component of this color.
                // @Example "0,0.33,0.66,1" .x returns "0.66".
                // -->1
                case "blue":
                    return new NumberTag(Internal.B / 255f).Handle(data.Shrink());
                // <--[tag]
                // @Name LocationTag.alpha
                // @Group General Information
                // @ReturnType NumberTag
                // @Returns the alpha component of this color.
                // @Example "0,0.33,0.66,1" .x returns "1".
                // -->1
                case "alpha":
                    return new NumberTag(Internal.A / 255f).Handle(data.Shrink());
                default:
                    return new TextTag(ToString()).Handle(data);
            }
        }

        public override string ToString()
        {
            return (Internal.R / 255f) + "," + (Internal.G / 255f) + "," + (Internal.B / 255f) + "," + (Internal.A / 255f);
        }

    }
}
