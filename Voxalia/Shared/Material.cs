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
                new MaterialInfo(0) { Solid = false, Opaque = false }, // AIR
                new MaterialInfo(1), // STONE
                new MaterialInfo(2), // GRASS
                new MaterialInfo(3) // DIRT
            };
            mats[2].TID[(int)MaterialSide.TOP] = MaterialHelpers.MAX_TEXTURES; // grass_top
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

        public static int TextureID(this Material mat, MaterialSide side)
        {
            return ALL_MATS[(int)mat].TID[(int)side];
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

        public int ID = 0;

        public bool Solid = true;

        public bool Opaque = true;

        public int[] TID = new int[(int)MaterialSide.COUNT];
    }
}
