using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.Shared
{
    public enum Material: ushort
    {
        AIR = 0,
        STONE = 1,
        GRASS = 2,
        DIRT = 3,
        NUM_DEFAULT = 4,
        MAX = ushort.MaxValue
    }

    public static class MaterialHelpers
    {
        public static int MAX_TEXTURES = 64;

        public static List<MaterialInfo> ALL_MATS = new List<MaterialInfo>((int)Material.NUM_DEFAULT);

        static MaterialHelpers()
        {
            MaterialInfo[] mats = new MaterialInfo[] {
                new MaterialInfo(0) { Solid = false, Opaque = false, RendersAtAll = false }, // AIR
                new MaterialInfo(1), // STONE
                new MaterialInfo(2), // GRASS
                new MaterialInfo(3) // DIRT
            };
            mats[2].TID[(int)MaterialSide.TOP] = MaterialHelpers.MAX_TEXTURES - 1; // grass_top
            mats[2].TID[(int)MaterialSide.BOTTOM] = 3; // dirt
            ALL_MATS.AddRange(mats);
        }
        
        public static bool IsSolid(this Material mat)
        {
            return ALL_MATS[(int)mat].Solid;
        }

        public static bool IsOpaque(this Material mat)
        {
            return ALL_MATS[(int)mat].Opaque;
        }

        public static bool RendersAtAll(this Material mat)
        {
            return ALL_MATS[(int)mat].RendersAtAll;
        }

        public static int TextureID(this Material mat, MaterialSide side)
        {
            return ALL_MATS[(int)mat].TID[(int)side];
        }

        public static string GetName(this Material mat)
        {
            string bname = ALL_MATS[(int)mat].Name;
            if (bname != null)
            {
                return bname;
            }
            return mat.ToString();
        }

        public static string GetDescription(this Material mat)
        {
            string bdesc = ALL_MATS[(int)mat].Description;
            if (bdesc != null)
            {
                return bdesc;
            }
            return "A standard block of " + mat.ToString().ToLower();
        }
    }

    public enum MaterialSide : byte
    {
        TOP = 0,
        BOTTOM = 1,
        XP = 2,
        YP = 3,
        XM = 4,
        YM = 5,
        COUNT = 6
    }

    public class MaterialInfo
    {
        public MaterialInfo(int _ID)
        {
            ID = _ID;
            for (int i = 0; i < (int)MaterialSide.COUNT; i++)
            {
                TID[i] = ID;
            }
        }

        public string Name = null;

        public string Description = null;

        public int ID = 0;

        public bool Solid = true;

        public bool Opaque = true;

        public bool RendersAtAll = true;

        public int[] TID = new int[(int)MaterialSide.COUNT];
    }
}
