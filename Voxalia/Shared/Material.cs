﻿using System;
using System.Collections.Generic;

namespace Voxalia.Shared
{
    public enum Material: ushort
    {
        AIR = 0,
        STONE = 1,
        GRASS_FOREST = 2,
        DIRT = 3,
        WATER = 4,
        DEBUG = 5,
        LEAVES1 = 6,
        CONCRETE = 7,
        SLIPGOO = 8,
        SNOW = 9,
        SMOKE = 10,
        LOG = 11,
        TALLGRASS = 12,
        SAND = 13,
        STEEL_SOLID = 14,
        STEEL_PLATE = 15,
        GRASS_PLAINS = 16,
        SANDSTONE = 17,
        TIN_ORE = 18,
        TIN_ORE_SPARSE = 19,
        COPPER_ORE = 20,
        COPPER_ORE_SPARSE = 21,
        NUM_DEFAULT = 22,
        MAX = ushort.MaxValue
    }

    public static class MaterialHelpers
    {
        public static int MAX_TEXTURES = 64; // Warning: Do not set this too high, this is used for texture block generation!

        public static List<MaterialInfo> ALL_MATS = new List<MaterialInfo>((int)Material.NUM_DEFAULT);

        static MaterialHelpers()
        {
            MaterialInfo[] mats = new MaterialInfo[] {
                new MaterialInfo((int)Material.AIR) { Solidity = MaterialSolidity.NONSOLID, Opaque = false, RendersAtAll = false, FogAlpha = 0 },
                new MaterialInfo((int)Material.STONE) { SpeedMod = 1.1f, Sound = MaterialSound.STONE, Hardness = 20 },
                new MaterialInfo((int)Material.GRASS_FOREST) { Sound = MaterialSound.GRASS },
                new MaterialInfo((int)Material.DIRT) { Sound = MaterialSound.DIRT },
                new MaterialInfo((int)Material.WATER) { Solidity = MaterialSolidity.LIQUID, Opaque = false, FogColor = new Location(0, 0.5, 1), Hardness = 5 },
                new MaterialInfo((int)Material.DEBUG) { Sound = MaterialSound.METAL, Hardness = 100 },
                new MaterialInfo((int)Material.LEAVES1) { Opaque = false, SpeedMod = 0.7f, FogAlpha = 0, CanRenderAgainstSelf = true, Sound = MaterialSound.LEAVES },
                new MaterialInfo((int)Material.CONCRETE) { SpeedMod = 1.2f, Sound = MaterialSound.STONE, Hardness = 25 },
                new MaterialInfo((int)Material.SLIPGOO) { Opaque = false, SpeedMod = 1.2f, FrictionMod = 0.01f, FogColor = new Location(0, 1, 0), Hardness = 5 },
                new MaterialInfo((int)Material.SNOW) { SpeedMod = 0.8f, Sound = MaterialSound.SNOW },
                new MaterialInfo((int)Material.SMOKE) { Solidity = MaterialSolidity.GAS, Opaque = false, FogColor = new Location(0.8) },
                new MaterialInfo((int)Material.LOG) { SpeedMod = 1.1f, Sound = MaterialSound.WOOD },
                new MaterialInfo((int)Material.TALLGRASS) { Solidity = MaterialSolidity.SPECIAL, Opaque = false, Hardness = 1 },
                new MaterialInfo((int)Material.SAND) { Sound = MaterialSound.SAND },
                new MaterialInfo((int)Material.STEEL_SOLID) { SpeedMod = 1.25f, Sound = MaterialSound.METAL, Hardness = 30 },
                new MaterialInfo((int)Material.STEEL_PLATE) { SpeedMod = 1.35f, Sound = MaterialSound.METAL, Hardness = 40 },
                new MaterialInfo((int)Material.GRASS_PLAINS) { Sound = MaterialSound.GRASS },
                new MaterialInfo((int)Material.SANDSTONE) { Sound = MaterialSound.STONE, Hardness = 15 },
                new MaterialInfo((int)Material.TIN_ORE) { Sound = MaterialSound.STONE, Hardness = 15 },
                new MaterialInfo((int)Material.TIN_ORE_SPARSE) { Sound = MaterialSound.STONE, Hardness = 15 },
                new MaterialInfo((int)Material.COPPER_ORE) { Sound = MaterialSound.STONE, Hardness = 15 },
                new MaterialInfo((int)Material.COPPER_ORE_SPARSE) { Sound = MaterialSound.STONE, Hardness = 15 },
            };
            mats[(int)Material.GRASS_FOREST].TID[(int)MaterialSide.TOP] = MAX_TEXTURES - 1; // grass (top)
            mats[(int)Material.GRASS_FOREST].TID[(int)MaterialSide.BOTTOM] = 3; // dirt
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.BOTTOM] = MAX_TEXTURES - 2; // db_bottom
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.XM] = MAX_TEXTURES - 3; // db_xm
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.XP] = MAX_TEXTURES - 4; // db_xp
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.YP] = MAX_TEXTURES - 5; // db_yp
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.YM] = MAX_TEXTURES - 6; // db_ym
            mats[(int)Material.LOG].TID[(int)MaterialSide.TOP] = MAX_TEXTURES - 7; // wood_top
            mats[(int)Material.LOG].TID[(int)MaterialSide.BOTTOM] = MAX_TEXTURES - 7; // wood_top
            mats[(int)Material.GRASS_PLAINS].TID[(int)MaterialSide.TOP] = MAX_TEXTURES - 8; // grass plains (top)
            mats[(int)Material.GRASS_PLAINS].TID[(int)MaterialSide.BOTTOM] = 3; // dirt
            ALL_MATS.AddRange(mats);
        }
        
        public static MaterialSolidity GetSolidity(this Material mat)
        {
            return ALL_MATS[(int)mat].Solidity;
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

        public static MaterialSound Sound(this Material mat)
        {
            return ALL_MATS[(int)mat].Sound;
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
        
        public static float GetSpeedMod(this Material mat)
        {
            return ALL_MATS[(int)mat].SpeedMod;
        }

        public static float GetFrictionMod(this Material mat)
        {
            return ALL_MATS[(int)mat].FrictionMod;
        }

        public static Location GetFogColor(this Material mat)
        {
            return ALL_MATS[(int)mat].FogColor;
        }

        public static float GetFogAlpha(this Material mat)
        {
            return ALL_MATS[(int)mat].FogAlpha;
        }

        public static float GetHardness(this Material mat)
        {
            return ALL_MATS[(int)mat].Hardness;
        }

        public static bool GetCanRenderAgainstSelf(this Material mat)
        {
            return ALL_MATS[(int)mat].CanRenderAgainstSelf;
        }

        public static Type MaterialType = typeof(Material);

        public static Material FromNameOrNumber(string input)
        {
            int t;
            if (int.TryParse(input, out t))
            {
                return (Material)t;
            }
            return (Material)Enum.Parse(MaterialType, input, true);
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

    public enum MaterialSound : byte
    {
        NONE = 0,
        GRASS = 1,
        SAND = 2,
        LEAVES = 3,
        METAL = 4,
        STONE = 5,
        SNOW = 6,
        DIRT = 7,
        WOOD = 8
    }

    [Flags]
    public enum MaterialSolidity : byte
    {
        NONSOLID = 1,
        FULLSOLID = 2,
        LIQUID = 4,
        GAS = 8,
        SPECIAL = 16,
        ANY = FULLSOLID | LIQUID | GAS | SPECIAL
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
        
        public int ID = 0;

        public float SpeedMod = 1f;
        
        public bool Opaque = true;

        public bool RendersAtAll = true;

        public bool CanRenderAgainstSelf = false;

        public float FrictionMod = 1f;

        public Location FogColor = new Location(0.7);

        public float FogAlpha = 1;

        public float Hardness = 10;

        public MaterialSound Sound = MaterialSound.NONE;

        public MaterialSolidity Solidity = MaterialSolidity.FULLSOLID;
        
        public int[] TID = new int[(int)MaterialSide.COUNT];
    }
}
