using System;
using System.Collections.Generic;
using Voxalia.Shared.Files;
using FreneticScript;

namespace Voxalia.Shared
{
    /// <summary>
    /// All defaultly available block materials.
    /// </summary>
    public enum Material: ushort
    {
        AIR = 0,
        STONE = 1,
        GRASS_FOREST = 2,
        DIRT = 3,
        WATER = 4,
        DEBUG = 5,
        LEAVES_OAK_SOLID = 6,
        CONCRETE = 7,
        SOLID_SLIME = 8,
        SNOW = 9,
        SMOKE = 10,
        LOG_OAK = 11,
        // Unused=12
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
        OIL = 28,
        ICE = 29,
        HEAVY_GAS = 30,
        LIQUID_SLIME = 31,
        LEAVES_OAK_LIQUID = 32,
        /// <summary>
        /// How many materials there are by default. Only for use with direct handling of this enumeration (shouldn't happen often.)
        /// </summary>
        NUM_DEFAULT = 33,
        /// <summary>
        /// How many materials there theoretically can be.
        /// </summary>
        MAX = 16384
    }

    /// <summary>
    /// Helps with material datas. Offers methods to material enumeration entries.
    /// </summary>
    public static class MaterialHelpers
    {
        public static int TextureCount = 5;

        public static bool Populated = false;

        public static void Populate(FileHandler files)
        {
            if (Populated)
            {
                return;
            }
            Populated = true;
            List<string> fileList = files.ListFiles("info/blocks/");
            foreach (string file in fileList)
            {
                string f = file.ToLowerFast().After("/blocks/").Before(".blk");
                Material mat;
                if (TryGetFromNameOrNumber(f, out mat))
                {
                    continue;
                }
                mat = (Material)Enum.Parse(typeof(Material), f.ToUpperInvariant());
                string data = files.ReadText("info/blocks/" + f + ".blk");
                string[] split = data.Replace("\r", "").Split('\n');
                MaterialInfo inf = new MaterialInfo((int)mat);
                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].StartsWith("//") || !split[i].Contains("="))
                    {
                        continue;
                    }
                    string[] opt = split[i].SplitFast('=', 2);
                    switch (opt[0].ToLowerFast())
                    {
                        case "name":
                            inf.SetName(opt[1]);
                            break;
                        case "sound":
                            inf.Sound = (MaterialSound)Enum.Parse(typeof(MaterialSound), opt[1].ToUpperInvariant());
                            break;
                        case "solidity":
                            inf.Solidity = (MaterialSolidity)Enum.Parse(typeof(MaterialSolidity), opt[1].ToUpperInvariant());
                            break;
                        case "speedmod":
                            inf.SpeedMod = float.Parse(opt[1]);
                            break;
                        case "frictionmod":
                            inf.FrictionMod = float.Parse(opt[1]);
                            break;
                        case "lightdamage":
                            inf.LightDamage = float.Parse(opt[1]);
                            break;
                        case "fogalpha":
                            inf.FogAlpha = float.Parse(opt[1]);
                            break;
                        case "opaque":
                            inf.Opaque = opt[1].ToLowerFast() == "true";
                            break;
                        case "spreads":
                            inf.Spreads = opt[1].ToLowerFast() == "true";
                            break;
                        case "rendersatall":
                            inf.RendersAtAll = opt[1].ToLowerFast() == "true";
                            break;
                        case "fogcolor":
                            inf.FogColor = Location.FromString(opt[1]);
                            break;
                        case "canrenderagainstself":
                            inf.CanRenderAgainstSelf = opt[1].ToLowerFast() == "true";
                            break;
                        case "hardness":
                            inf.Hardness = float.Parse(opt[1]);
                            break;
                        case "breaktime":
                            inf.BreakTime = opt[1].ToLowerFast() == "infinity" ? float.PositiveInfinity : float.Parse(opt[1]);
                            break;
                        case "breaker":
                            inf.Breaker = (MaterialBreaker)Enum.Parse(typeof(MaterialBreaker), opt[1].ToUpperInvariant());
                            break;
                        case "texture_top":
                            inf.TID[(int)MaterialSide.TOP] = ParseTID(opt[1]);
                            break;
                        case "texture_bottom":
                            inf.TID[(int)MaterialSide.BOTTOM] = ParseTID(opt[1]);
                            break;
                        case "texture_xp":
                            inf.TID[(int)MaterialSide.XP] = ParseTID(opt[1]);
                            break;
                        case "texture_xm":
                            inf.TID[(int)MaterialSide.XM] = ParseTID(opt[1]);
                            break;
                        case "texture_yp":
                            inf.TID[(int)MaterialSide.YP] = ParseTID(opt[1]);
                            break;
                        case "texture_ym":
                            inf.TID[(int)MaterialSide.YM] = ParseTID(opt[1]);
                            break;
                        default:
                            throw new Exception("Invalid option: " + opt[0]);
                    }
                }
                while (ALL_MATS.Count <= (int)mat)
                {
                    ALL_MATS.Add(null);
                }
                ALL_MATS[(int)mat] = inf;
                TextureCount++;
            }
            TextureCount += TMC;
            for (int i = 0; i < ALL_MATS.Count; i++)
            {
                if (ALL_MATS[i] != null)
                {
                    for (int s = 0; s < (int)MaterialSide.COUNT; s++)
                    {
                        if (ALL_MATS[i].TID[s] > short.MaxValue)
                        {
                            ALL_MATS[i].TID[s] = TextureCount - (ALL_MATS[i].TID[s] - short.MaxValue);
                        }
                    }
                }
            }
        }

        static int TMC = 0;

        static int ParseTID(string str)
        {
            int min;
            if (str.StartsWith("m") && int.TryParse(str.Substring(1), out min))
            {
                if (TMC < min)
                {
                    TMC = min;
                }
                return short.MaxValue + min;
            }
            Material mat = (Material)Enum.Parse(typeof(Material), str.ToUpperInvariant());
            return (int)mat;
        }
        
        /// <summary>
        /// All material data known to this engine.
        /// </summary>
        public static List<MaterialInfo> ALL_MATS = new List<MaterialInfo>((int)Material.NUM_DEFAULT);

        public static bool IsValid(Material mat)
        {
            return (int)mat < ALL_MATS.Count && ALL_MATS[(int)mat] != null;
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

        public static MaterialBreaker GetBreaker(this Material mat)
        {
            return ALL_MATS[(int)mat].Breaker;
        }

        public static Type MaterialType = typeof(Material);

        public static bool TryGetFromNameOrNumber(string input, out Material mat)
        {
            ushort t;
            if (ushort.TryParse(input, out t))
            {
                mat = (Material)t;
                return true;
            }
            string inp = input.ToUpperInvariant();
            int hash = inp.GetHashCode();
            for (t = 0; t < ALL_MATS.Count; t++)
            {
                if (ALL_MATS[t] != null && ALL_MATS[t].NameHash == hash && ALL_MATS[t].Name == inp)
                {
                    mat = (Material)t;
                    return true;
                }
            }
            mat = Material.AIR;
            return false;
        }

        public static Material FromNameOrNumber(string input)
        {
            Material mat;
            if (TryGetFromNameOrNumber(input, out mat))
            {
                return mat;
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
        GLASS = 9,
        // TODO: Clay?
        /// <summary>
        /// How many total material-generated sound types there are.
        /// </summary>
        COUNT = 10
    }

    public enum MaterialBreaker
    {
        HAND = 1,
        PICKAXE = 2,
        AXE = 3,
        SHOVEL = 4
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

        public MaterialBreaker Breaker = MaterialBreaker.HAND;
    }
}
