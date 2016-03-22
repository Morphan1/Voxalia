using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Voxalia.Shared
{
    /// <summary>
    /// Contains the list of all block paint colors. Colors can be reused for other things.
    /// </summary>
    public static class Colors
    {
        public static Color WHITE = Color.FromArgb(255, 255, 255);
        public static Color BLACK = Color.FromArgb(0, 0, 0);
        public static Color GREEN = Color.FromArgb(0, 255, 0);
        public static Color BLUE = Color.FromArgb(0, 0, 255);
        public static Color RED = Color.FromArgb(255, 0, 0);
        public static Color MAGENTA = Color.FromArgb(255, 0, 255);
        public static Color YELLOW = Color.FromArgb(255, 255, 0);
        public static Color CYAN = Color.FromArgb(0, 255, 255);
        public static Color DARK_GREEN = Color.FromArgb(0, 128, 0);
        public static Color DARK_BLUE = Color.FromArgb(0, 0, 128);
        public static Color DARK_RED = Color.FromArgb(128, 0, 0);
        public static Color LIGHT_GREEN = Color.FromArgb(128, 255, 128);
        public static Color LIGHT_BLUE = Color.FromArgb(128, 128, 255);
        public static Color LIGHT_RED = Color.FromArgb(255, 128, 128);
        public static Color GRAY = Color.FromArgb(128, 128, 128);
        public static Color LIGHT_GRAY = Color.FromArgb(192, 192, 192);
        public static Color DARK_GRAY = Color.FromArgb(64, 64, 64);
        public static Color DARK_MAGENTA = Color.FromArgb(128, 0, 128);
        public static Color DARK_YELLOW = Color.FromArgb(128, 128, 0);
        public static Color DARK_CYAN = Color.FromArgb(0, 128, 128);
        public static Color LIGHT_MAGENTA = Color.FromArgb(255, 128, 255);
        public static Color LIGHT_YELLOW = Color.FromArgb(255, 255, 128);
        public static Color LIGHT_CYAN = Color.FromArgb(128, 255, 255);
        public static Color ORANGE = Color.FromArgb(255, 128, 0);
        public static Color BROWN = Color.FromArgb(128, 64, 0);
        public static Color PURPLE = Color.FromArgb(128, 0, 255);
        public static Color PINK = Color.FromArgb(255, 128, 255);
        public static Color LIME = Color.FromArgb(128, 255, 0);
        public static Color SKY_BLUE = Color.FromArgb(0, 128, 255);
        public static Color VERY_DARK_GRAY = Color.FromArgb(32, 32, 32);
        public static Color OLD_MAGIC = Color.FromArgb(0, 0, 0, 0);
        public static Color RAINBOW = Color.FromArgb(0, 255, 0, 255);
        public static Color SLIGHTLY_TRANSPARENT = Color.FromArgb(191, 255, 255, 255);
        public static Color TRANSPARENT = Color.FromArgb(127, 255, 255, 255);
        public static Color VERY_TRANSPARENT = Color.FromArgb(63, 255, 255, 255);

        public static Dictionary<string, byte> KnownColorNames = new Dictionary<string, byte>();

        public static Color[] KnownColorsArray = new Color[64];

        public static string[] KnownColorNamesArray = new string[64];

        public static Color ForByte(byte input)
        {
            int baseinp = input;
            return KnownColorsArray[baseinp];
        }

        public static string NameForByte(byte input)
        {
            int baseinp = input;
            return KnownColorNamesArray[baseinp];
        }
        
        public static byte ForName(string name)
        {
            byte val;
            if (byte.TryParse(name, out val))
            {
                return val;
            }
            name = name.ToUpperInvariant();
            if (KnownColorNames.TryGetValue(name, out val))
            {
                return val;
            }
            return 0;
        }

        static int inc = 0;

        public static int TRANS1;
        public static int TRANS2;

        static int Register(string name, Color col)
        {
            KnownColorNames.Add(name, (byte)inc);
            KnownColorNamesArray[inc] = name;
            KnownColorsArray[inc] = col;
            return inc++;
        }

        static Colors()
        {
            Register("WHITE", WHITE);
            Register("BLACK", BLACK);
            Register("GREEN", GREEN);
            Register("BLUE", BLUE);
            Register("RED", RED);
            Register("MAGENTA", MAGENTA);
            Register("YELLOW", YELLOW);
            Register("CYAN", CYAN);
            Register("DARK_GREEN", DARK_GREEN);
            Register("DARK_BLUE", DARK_BLUE);
            Register("DARK_RED", DARK_RED);
            Register("LIGHT_GREEN", LIGHT_GREEN);
            Register("LIGHT_BLUE", LIGHT_BLUE);
            Register("LIGHT_RED", LIGHT_RED);
            Register("GRAY", GRAY);
            Register("LIGHT_GRAY", LIGHT_GRAY);
            Register("DARK_GRAY", DARK_GRAY);
            Register("DARK_MAGENTA", DARK_MAGENTA);
            Register("DARK_YELLOW", DARK_YELLOW);
            Register("DARK_CYAN", DARK_CYAN);
            Register("LIGHT_MAGENTA", LIGHT_MAGENTA);
            Register("LIGHT_YELLOW", LIGHT_YELLOW);
            Register("LIGHT_CYAN", LIGHT_CYAN);
            Register("ORANGE", ORANGE);
            Register("BROWN", BROWN);
            Register("PURPLE", PURPLE);
            Register("PINK", PINK);
            Register("LIME", LIME);
            Register("SKY_BLUE", SKY_BLUE);
            Register("VERY_DARK_GRAY", VERY_DARK_GRAY);
            Register("PLACEHOLDER_29", WHITE);
            Register("PLACEHOLDER_28", WHITE);
            Register("PLACEHOLDER_27", WHITE);
            Register("PLACEHOLDER_26", WHITE);
            Register("PLACEHOLDER_25", WHITE);
            Register("PLACEHOLDER_24", WHITE);
            Register("PLACEHOLDER_23", WHITE);
            Register("PLACEHOLDER_22", WHITE);
            Register("PLACEHOLDER_21", WHITE);
            Register("PLACEHOLDER_20", WHITE);
            Register("PLACEHOLDER_19", WHITE);
            Register("PLACEHOLDER_18", WHITE);
            Register("PLACEHOLDER_17", WHITE);
            Register("PLACEHOLDER_16", WHITE);
            Register("PLACEHOLDER_15", WHITE);
            Register("PLACEHOLDER_14", WHITE);
            Register("PLACEHOLDER_13", WHITE);
            Register("PLACEHOLDER_12", WHITE);
            Register("PLACEHOLDER_11", WHITE);
            Register("PLACEHOLDER_10", WHITE);
            Register("PLACEHOLDER_9", WHITE);
            Register("PLACEHOLDER_8", WHITE);
            Register("PLACEHOLDER_7", WHITE);
            Register("PLACEHOLDER_6", WHITE);
            Register("PLACEHOLDER_5", WHITE);
            Register("PLACEHOLDER_4", WHITE);
            Register("PLACEHOLDER_3", WHITE);
            Register("PLACEHOLDER_2", WHITE);
            Register("PLACEHOLDER_1", WHITE);
            TRANS1 = Register("SLIGHTLY_TRANSPARENT", SLIGHTLY_TRANSPARENT);
            Register("TRANSPARENT", TRANSPARENT);
            TRANS2 = Register("VERY_TRANSPARENT", VERY_TRANSPARENT);
            Register("OLD_MAGIC", OLD_MAGIC);
            Register("RAINBOW", RAINBOW);
        }
    }
}
