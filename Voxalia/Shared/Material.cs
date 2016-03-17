using System;
using System.Collections.Generic;

namespace Voxalia.Shared
{
    /// <summary>
    /// All available block materials.
    /// </summary>
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
        LOG_OAK = 11,
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
        COAL_ORE = 22,
        COAL_ORE_SPARSE = 23,
        COLOR = 24,
        DIRTY_WATER = 25,
        PLANKS_OAK = 26,
        GLASS_WINDOW = 27,
        /// <summary>
        /// How many materials there are by default. Only for use with internal pre-generation, or direct handling of this enumeration (shouldn't happen often.)
        /// </summary>
        NUM_DEFAULT = 28,
        /// <summary>
        /// How many materials there theoretically can be.
        /// </summary>
        MAX = ushort.MaxValue
    }

    /// <summary>
    /// Helps with material datas. Offers methods to material enumeration entries.
    /// </summary>
    public static class MaterialHelpers
    {
        /// <summary>
        /// Roughly how many materials should be expected during generation, plus several extras for spare room.
        /// Warning: Do not set this too high, this is used for texture block generation, and this * texture_size^2 will be taken in the vRAM!
        /// For the same reason, do not set it too low!
        /// </summary>
        public static int MAX_THEORETICAL_MATERIALS = 64;

        /// <summary>
        /// All material data known to this engine.
        /// </summary>
        public static List<MaterialInfo> ALL_MATS = new List<MaterialInfo>((int)Material.NUM_DEFAULT);

        static MaterialHelpers()
        {
            MaterialInfo[] mats = new MaterialInfo[] {
                new MaterialInfo((int)Material.AIR) { Solidity = MaterialSolidity.NONSOLID, Opaque = false, RendersAtAll = false, FogAlpha = 0, BreakTime = float.PositiveInfinity, LightDamage = 0f },
                new MaterialInfo((int)Material.STONE) { SpeedMod = 1.1f, Sound = MaterialSound.STONE, Hardness = 20, BreakTime = 2 },
                new MaterialInfo((int)Material.GRASS_FOREST) { Sound = MaterialSound.GRASS, BreakTime = 1 },
                new MaterialInfo((int)Material.DIRT) { Sound = MaterialSound.DIRT, BreakTime = 1 },
                new MaterialInfo((int)Material.WATER) { Solidity = MaterialSolidity.LIQUID, Opaque = false, FogColor = new Location(0, 0.5, 1), Hardness = 5, BreakTime = 0.2f, Spreads = true, LightDamage = 0.28f },
                new MaterialInfo((int)Material.DEBUG) { Sound = MaterialSound.METAL, Hardness = 100, BreakTime = 2f },
                new MaterialInfo((int)Material.LEAVES1) { Opaque = false, SpeedMod = 0.7f, FogAlpha = 0, CanRenderAgainstSelf = true, Sound = MaterialSound.LEAVES, BreakTime = 0.5f },
                new MaterialInfo((int)Material.CONCRETE) { SpeedMod = 1.2f, Sound = MaterialSound.STONE, Hardness = 25, BreakTime = 3f },
                new MaterialInfo((int)Material.SLIPGOO) { Opaque = false, SpeedMod = 1.2f, FrictionMod = 0.01f, FogColor = new Location(0, 1, 0), Hardness = 5, BreakTime = 1 },
                new MaterialInfo((int)Material.SNOW) { SpeedMod = 0.8f, Sound = MaterialSound.SNOW, BreakTime = 0.5f },
                new MaterialInfo((int)Material.SMOKE) { Solidity = MaterialSolidity.GAS, Opaque = false, FogColor = new Location(0.8), BreakTime = 0.2f, LightDamage = 0.1f },
                new MaterialInfo((int)Material.LOG_OAK) { SpeedMod = 1.1f, Sound = MaterialSound.WOOD, BreakTime = 1.5f },
                new MaterialInfo((int)Material.TALLGRASS) { Solidity = MaterialSolidity.SPECIAL, Opaque = false, Hardness = 1, BreakTime = 0.2f },
                new MaterialInfo((int)Material.SAND) { Sound = MaterialSound.SAND, BreakTime = 0.5f },
                new MaterialInfo((int)Material.STEEL_SOLID) { SpeedMod = 1.25f, Sound = MaterialSound.METAL, Hardness = 30, BreakTime = 5 },
                new MaterialInfo((int)Material.STEEL_PLATE) { SpeedMod = 1.35f, Sound = MaterialSound.METAL, Hardness = 40, BreakTime = 5 },
                new MaterialInfo((int)Material.GRASS_PLAINS) { Sound = MaterialSound.GRASS, BreakTime = 1 },
                new MaterialInfo((int)Material.SANDSTONE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 1.5f },
                new MaterialInfo((int)Material.TIN_ORE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.TIN_ORE_SPARSE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.COPPER_ORE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.COPPER_ORE_SPARSE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.COAL_ORE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.COAL_ORE_SPARSE) { Sound = MaterialSound.STONE, Hardness = 15, BreakTime = 3 },
                new MaterialInfo((int)Material.COLOR) { Sound = MaterialSound.STONE, Hardness = 5, BreakTime = 1 }, // TODO: Clay sound?
                new MaterialInfo((int)Material.DIRTY_WATER) { Solidity = MaterialSolidity.LIQUID, Opaque = false, FogColor = new Location(0, 0.5, 0.25), Hardness = 5, BreakTime = 0.2f, Spreads = true, LightDamage = 0.35f },
                new MaterialInfo((int)Material.PLANKS_OAK) { SpeedMod = 1.15f, Sound = MaterialSound.WOOD, BreakTime = 1f },
                new MaterialInfo((int)Material.GLASS_WINDOW) { SpeedMod = 1.1f, Sound = MaterialSound.METAL, Hardness = 5, BreakTime = 1, Opaque = false, LightDamage = 0.05f },
            };
            mats[(int)Material.GRASS_FOREST].TID[(int)MaterialSide.TOP] = MAX_THEORETICAL_MATERIALS - 1; // grass (top)
            mats[(int)Material.GRASS_FOREST].TID[(int)MaterialSide.BOTTOM] = 3; // dirt
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.BOTTOM] = MAX_THEORETICAL_MATERIALS - 2; // db_bottom
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.XM] = MAX_THEORETICAL_MATERIALS - 3; // db_xm
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.XP] = MAX_THEORETICAL_MATERIALS - 4; // db_xp
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.YP] = MAX_THEORETICAL_MATERIALS - 5; // db_yp
            mats[(int)Material.DEBUG].TID[(int)MaterialSide.YM] = MAX_THEORETICAL_MATERIALS - 6; // db_ym
            mats[(int)Material.LOG_OAK].TID[(int)MaterialSide.TOP] = MAX_THEORETICAL_MATERIALS - 7; // wood_top
            mats[(int)Material.LOG_OAK].TID[(int)MaterialSide.BOTTOM] = MAX_THEORETICAL_MATERIALS - 7; // wood_top
            mats[(int)Material.GRASS_PLAINS].TID[(int)MaterialSide.TOP] = MAX_THEORETICAL_MATERIALS - 8; // grass plains (top)
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

        public static bool ShouldSpread(this Material mat)
        {
            return ALL_MATS[(int)mat].Spreads;
        }

        public static float GetBreakTime(this Material mat)
        {
            return ALL_MATS[(int)mat].BreakTime;
        }

        public static float GetLightDamage(this Material mat)
        {
            return ALL_MATS[(int)mat].LightDamage;
        }

        public static Type MaterialType = typeof(Material);

        public static Material FromNameOrNumber(string input)
        {
            ushort t;
            if (ushort.TryParse(input, out t))
            {
                return (Material)t;
            }
            string inp = input.ToUpperInvariant();
            int hash = inp.GetHashCode();
            for (t = 0; t < ALL_MATS.Count; t++)
            {
                if (ALL_MATS[t].NameHash == hash && ALL_MATS[t].Name == inp)
                {
                    return (Material)t;
                }
            }
            throw new Exception("Unknown material name or ID: " + input);
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

    /// <summary>
    /// An enumeration of potential sound sets any material can choose from.
    /// </summary>
    public enum MaterialSound : byte
    {
        /// <summary>
        /// No sound is generated by this material. Should not be actually used.
        /// </summary>
        NONE = 0,
        GRASS = 1,
        SAND = 2,
        LEAVES = 3,
        METAL = 4,
        STONE = 5,
        SNOW = 6,
        DIRT = 7,
        WOOD = 8,
        // TODO: Clay?
        // TODO: Glass?
        /// <summary>
        /// How many total material-generated sound types there are.
        /// </summary>
        COUNT = 9
    }

    /// <summary>
    /// Represents the possible solidity modes of a material, as flags. Use HasFlag to check if a certain solidity is in use!
    /// </summary>
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

    /// <summary>
    /// Represents a set of data related to a material. Access via through MaterialHelpers.
    /// </summary>
    public class MaterialInfo
    {
        public MaterialInfo(int _ID)
        {
            ID = _ID;
            for (int i = 0; i < (int)MaterialSide.COUNT; i++)
            {
                TID[i] = ID;
            }
            SetName(((Material)ID).ToString());
        }

        public void SetName(string name)
        {
            Name = name.ToUpperInvariant();
            NameHash = Name.GetHashCode();
        }

        public string Name = null;

        public int NameHash = 0;
        
        public int ID = 0;

        public float SpeedMod = 1f;
        
        public bool Opaque = true;

        public bool RendersAtAll = true;

        public bool CanRenderAgainstSelf = false;

        public float FrictionMod = 1f;

        public Location FogColor = new Location(0.7);

        public float FogAlpha = 1;

        public float Hardness = 10;

        public float BreakTime = 1f;

        public float LightDamage = 1f;

        public MaterialSound Sound = MaterialSound.NONE;

        public MaterialSolidity Solidity = MaterialSolidity.FULLSOLID;

        public bool Spreads = false;
        
        public int[] TID = new int[(int)MaterialSide.COUNT];
    }
}
