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
        MAX = (ushort)(1 | 2 | 4 | 8 | 16 | 32 | 64 | 128 | 256 | 256 * 2 | 256 * 4),
        SUBDATA_BIT = (ushort)(256 * 8 | 256 * 16 | 256 * 32 | 256 * 64 | 256 * 128)
    }

    public static class MaterialHelpers
    {
        public static ushort MAX_BIT = (ushort)Material.MAX;
        public static ushort SUBDATA_BIT = (ushort)Material.SUBDATA_BIT;

        public static List<MaterialInfo> ALL_MATS = new List<MaterialInfo>((int)Material.NUM_DEFAULT);

        static MaterialHelpers()
        {
            MaterialInfo[] mats = new MaterialInfo[] {
                new MaterialInfo(0) { Solid = false, Opaque = false },
                new MaterialInfo(1) { Solid = true, Opaque = true }
            };
            ALL_MATS.AddRange(mats);
        }

        public static ushort GetMaterialSubData(ushort input)
        {
            return (ushort)(input & SUBDATA_BIT);
        }

        public static ushort GetMaterialHardMat(ushort input)
        {
            return (ushort)(input & MAX_BIT);
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

        public static Material GetHardMaterial(this Material mat)
        {
            return (Material)GetMaterialHardMat((ushort)mat);
        }

        public static ushort GetSubData(this Material mat)
        {
            return GetMaterialSubData((ushort)mat);
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
