using System;
using System.Collections.Generic;
using System.Diagnostics;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public class SimpleGeneratorCore: BlockPopulator
    {
        public SimpleBiomeGenerator Biomes = new SimpleBiomeGenerator();

        public const float GlobalHeightMapSize = 400;

        public const float LocalHeightMapSize = 40;

        public const float SolidityMapSize = 100;
        
        public const float OreMapSize = 70;

        public const float OreTypeMapSize = 150;

        public const float OreMapTolerance = 0.90f;

        public const float OreMapThickTolerance = 0.94f;
        
        public Material GetMatType(int seed2, int seed3, int seed4, int seed5, int x, int y, int z)
        {
            // TODO: better non-simplex code!
            float val = SimplexNoise.Generate((float)seed2 + (x / OreMapSize), (float)seed5 + (y / OreMapSize), (float)seed4 + (z / OreMapSize));
            if (val < OreMapTolerance)
            {
                return Material.AIR;
            }
            bool thick = val > OreMapThickTolerance;
            float tval = SimplexNoise.Generate((float)seed5 + (x / OreTypeMapSize), (float)seed3 + (y / OreTypeMapSize), (float)seed2 + (z / OreTypeMapSize));
            if (thick)
            {
                if (tval > 0.66f)
                {
                    return Material.TIN_ORE;
                }
                else if (tval > 0.33f)
                {
                    return Material.COAL_ORE;
                }
                else
                {
                    return Material.COPPER_ORE;
                }
            }
            else
            {
                if (tval > 0.66f)
                {
                    return Material.TIN_ORE_SPARSE;
                }
                else if (tval > 0.33f)
                {
                    return Material.COAL_ORE_SPARSE;
                }
                else
                {
                    return Material.COPPER_ORE_SPARSE;
                }
            }
        }

        public override void ClearTimings()
        {
#if TIMINGS
            Timings_Height = 0;
            Timings_Chunk = 0;
            Timings_Entities = 0;
#endif
        }

#if TIMINGS
        public double Timings_Height = 0;
        public double Timings_Chunk = 0;
        public double Timings_Entities = 0;
#endif

        public override List<Tuple<string, double>> GetTimings()
        {
            List<Tuple<string, double>> res = new List<Tuple<string, double>>();
#if TIMINGS
            res.Add(new Tuple<string, double>("Height", Timings_Height));
            res.Add(new Tuple<string, double>("Chunk", Timings_Chunk));
            res.Add(new Tuple<string, double>("Entities", Timings_Entities));
#endif
            return res;
        }

        public bool CanBeSolid(int seed3, int seed4, int seed5, int x, int y, int z, SimpleBiome biome)
        {
            // TODO: better non-simplex code?!
            float val = SimplexNoise.Generate((float)seed3 + (x / SolidityMapSize), (float)seed4 + (y / SolidityMapSize), (float)seed5 + (z / SolidityMapSize));
            //SysConsole.Output(OutputType.INFO, seed3 + "," + seed4 + "," + seed5 + " -> " + x + ", " + y + ", " + z + " -> " + val);
            return val < biome.AirDensity();
        }

        public float GetHeightQuick(int Seed, int seed2, float x, float y)
        {
            float lheight = SimplexNoise.Generate((float)seed2 + (x / GlobalHeightMapSize), (float)Seed + (y / GlobalHeightMapSize)) * 50f - 10f;
            float height = SimplexNoise.Generate((float)Seed + (x / LocalHeightMapSize), (float)seed2 + (y / LocalHeightMapSize)) * 6f - 3f;
            return lheight + height;
        }

        // TODO: Clean, reduce to only what's necessary. Every entry in this list makes things that much more expensive.
        /*public static Vector3i[] Rels = new Vector3i[] { new Vector3i(-10, 10, 0), new Vector3i(-10, -10, 0), new Vector3i(10, 10, 0), new Vector3i(10, -10, 0),
        new Vector3i(-15, 15, 0), new Vector3i(-15, -15, 0), new Vector3i(15, 15, 0), new Vector3i(15, -15, 0),
        new Vector3i(-20, 20, 0), new Vector3i(-20, -20, 0), new Vector3i(20, 20, 0), new Vector3i(20, -20, 0),
        new Vector3i(-5, 5, 0), new Vector3i(-5, -5, 0), new Vector3i(5, 5, 0), new Vector3i(5, -5, 0),
        new Vector3i(-10, 0, 0), new Vector3i(0, -10, 0), new Vector3i(10, 0, 0), new Vector3i(0, 10, 0),
        new Vector3i(-20, 0, 0), new Vector3i(0, -20, 0), new Vector3i(20, 0, 0), new Vector3i(0, 20, 0),
        new Vector3i(-15, 0, 0), new Vector3i(0, -15, 0), new Vector3i(15, 0, 0), new Vector3i(0, 15, 0),
        new Vector3i(-5, 0, 0), new Vector3i(0, -5, 0), new Vector3i(5, 0, 0), new Vector3i(0, 5, 0)};

        float relmod = 1f;*/

        public override float GetHeight(int Seed, int seed2, int seed3, int seed4, int seed5, float x, float y, float z, out Biome biome)
        {
#if TIMINGS
            Stopwatch sw = new Stopwatch();
            sw.Start();
            try
#endif
            {
                float valBasic = GetHeightQuick(Seed, seed2, x, y);
                //if (z < -50 || z > 50)
                {
                    biome = Biomes.BiomeFor(seed2, seed3, seed4, x, y, z, valBasic);
                    return valBasic;
                }
                /*Biome b = Biomes.BiomeFor(seed2, seed3, seed4, x, y, z, valBasic);
                float total = valBasic; * ((SimpleBiome)b).HeightMod();
                foreach (Vector3i vecer in Rels)
                {
                    float valt = GetHeightQuick(Seed, seed2, x + vecer.X * relmod, y + vecer.Y * relmod);
                    Biome bt = Biomes.BiomeFor(seed2, seed3, seed4, x + vecer.X * relmod, y + vecer.Y * relmod, z, valt);
                    total += valt * ((SimpleBiome)bt).HeightMod();
                }
                biome = b;
                return total / (Rels.Length + 1);
                */
            }
#if TIMINGS
            finally
            {
                sw.Stop();
                Timings_Height += sw.ElapsedTicks / (double)Stopwatch.Frequency;
            }
#endif
        }

        void SpecialSetBlockAt(Chunk chunk, int X, int Y, int Z, BlockInternal bi)
        {
            if (X < 0 || Y < 0 || Z < 0 || X >= Chunk.CHUNK_SIZE || Y >= Chunk.CHUNK_SIZE || Z >= Chunk.CHUNK_SIZE)
            {
                Vector3i chloc = chunk.OwningRegion.ChunkLocFor(new Location(X, Y, Z));
                Chunk ch = chunk.OwningRegion.LoadChunkNoPopulate(chunk.WorldPosition + chloc);
                int x = (int)(X - chloc.X * Chunk.CHUNK_SIZE);
                int y = (int)(Y - chloc.Y * Chunk.CHUNK_SIZE);
                int z = (int)(Z - chloc.Z * Chunk.CHUNK_SIZE);
                BlockInternal orig = ch.GetBlockAt(x, y, z);
                BlockFlags flags = ((BlockFlags)orig.BlockLocalData);
                if (!flags.HasFlag(BlockFlags.EDITED) && !flags.HasFlag(BlockFlags.PROTECTED))
                {
                    // TODO: lock?
                    ch.BlocksInternal[chunk.BlockIndex(x, y, z)] = bi;
                }
            }
            else
            {
                BlockInternal orig = chunk.GetBlockAt(X, Y, Z);
                BlockFlags flags = ((BlockFlags)orig.BlockLocalData);
                if (!flags.HasFlag(BlockFlags.EDITED) && !flags.HasFlag(BlockFlags.PROTECTED))
                {
                    chunk.BlocksInternal[chunk.BlockIndex(X, Y, Z)] = bi;
                }
            }
        }

        public byte[] OreShapes = new byte[] { 0, 64, 65, 66, 67, 68 };

        public int MaxNonAirHeight = 2;

        public override void Populate(int Seed, int seed2, int seed3, int seed4, int seed5, Chunk chunk)
        {
#if TIMINGS
            Stopwatch sw = new Stopwatch();
            sw.Start();
            PopulateInternal(Seed, seed2, seed3, seed4, seed5, chunk);
            sw.Stop();
            Timings_Chunk += sw.ElapsedTicks / (double)Stopwatch.Frequency;
        }

        private void PopulateInternal(int Seed, int seed2, int seed3, int seed4, int seed5, Chunk chunk)
        {
#endif
            if (chunk.WorldPosition.Z > MaxNonAirHeight)
            {
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    chunk.BlocksInternal[i] = BlockInternal.AIR;
                }
                return;
            }
            // TODO: Special case for too far down as well.
            for (int x = 0; x < Chunk.CHUNK_SIZE; x++)
            {
                for (int y = 0; y < Chunk.CHUNK_SIZE; y++)
                {
                    Location cpos = chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE;
                    // Prepare basics
                    int cx = (int)cpos.X + x;
                    int cy = (int)cpos.Y + y;
                    Biome biomeOrig;
                    float hheight = GetHeight(Seed, seed2, seed3, seed4, seed5, cx, cy, (float)cpos.Z, out biomeOrig);
                    SimpleBiome biome = (SimpleBiome)biomeOrig;
                    //Biome biomeOrig2;
                    /*float hheight2 = */
                    /*GetHeight(Seed, seed2, seed3, seed4, seed5, cx + 7, cy + 7, (float)cpos.Z + 7, out biomeOrig2);
                    SimpleBiome biome2 = (SimpleBiome)biomeOrig2;*/
                    Material surf = biome.SurfaceBlock();
                    Material seco = biome.SecondLayerBlock();
                    Material basb = biome.BaseBlock();
                    Material water = biome.WaterMaterial();
                    /*Material surf2 = biome2.SurfaceBlock();
                    Material seco2 = biome2.SecondLayerBlock();
                    Material basb2 = biome2.BaseBlock();*/
                    // TODO: Make this possible?: hheight = (hheight + hheight2) / 2f;
                    int hheightint = (int)Math.Round(hheight);
                    float topf = hheight - (float)(chunk.WorldPosition.Z * Chunk.CHUNK_SIZE);
                    int top = (int)Math.Round(topf);
                    // General natural ground
                    for (int z = 0; z < Math.Min(top - 5, Chunk.CHUNK_SIZE); z++)
                    {
                        if (CanBeSolid(seed3, seed4, seed5, cx, cy, (int)cpos.Z + z, biome))
                        {
                            Material typex = GetMatType(seed2, seed3, seed4, seed5, cx, cy, (int)cpos.Z + z);
                            byte shape = 0;
                            if (typex != Material.AIR)
                            {
                                shape = OreShapes[new Random((int)((hheight + cx + cy + cpos.Z + z) * 5)).Next(OreShapes.Length)];
                            }
                            //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z + z) / 10f) >= 0.5f;
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)(typex == Material.AIR ? (/*choice ? basb2 : */basb) : typex), shape, 0, 0);
                        }
                        else if ((CanBeSolid(seed3, seed4, seed5, cx, cy, (int)cpos.Z + z - 1, biome) || (CanBeSolid(seed3, seed4, seed5, cx, cy, (int)cpos.Z + z + 1, biome))) &&
                            (CanBeSolid(seed3, seed4, seed5, cx + 1, cy, (int)cpos.Z + z, biome) || CanBeSolid(seed3, seed4, seed5, cx, cy + 1, (int)cpos.Z + z, biome)
                                || CanBeSolid(seed3, seed4, seed5, cx - 1, cy, (int)cpos.Z + z, biome) || CanBeSolid(seed3, seed4, seed5, cx, cy - 1, (int)cpos.Z + z, biome)))
                        {
                            //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z + z) / 10f) >= 0.5f;
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)(/*choice ? basb2 : */basb), 3, 0, 0);
                        }
                    }
                    for (int z = Math.Max(top - 5, 0); z < Math.Min(top - 1, Chunk.CHUNK_SIZE); z++)
                    {
                        if (CanBeSolid(seed3, seed4, seed5, cx, cy, (int)cpos.Z + z, biome))
                        {
                            //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z + z) / 10f) >= 0.5f;
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)(/*choice ? seco2 :*/ seco), 0, 0, 0);
                        }
                    }
                    for (int z = Math.Max(top - 1, 0); z < Math.Min(top, Chunk.CHUNK_SIZE); z++)
                    {
                        //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z + z) / 10f) >= 0.5f;
                        chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)(/*choice ? surf2 : */surf), 0, 0, 0);
                    }
                    // Smooth terrain cap
                    Biome tempb;
                    float heightfxp = GetHeight(Seed, seed2, seed3, seed4, seed5, cx + 1, cy, (float)cpos.Z, out tempb);
                    float heightfxm = GetHeight(Seed, seed2, seed3, seed4, seed5, cx - 1, cy, (float)cpos.Z, out tempb);
                    float heightfyp = GetHeight(Seed, seed2, seed3, seed4, seed5, cx, cy + 1, (float)cpos.Z, out tempb);
                    float heightfym = GetHeight(Seed, seed2, seed3, seed4, seed5, cx, cy - 1, (float)cpos.Z, out tempb);
                    float topfxp = heightfxp - (float)chunk.WorldPosition.Z * Chunk.CHUNK_SIZE;
                    float topfxm = heightfxm - (float)chunk.WorldPosition.Z * Chunk.CHUNK_SIZE;
                    float topfyp = heightfyp - (float)chunk.WorldPosition.Z * Chunk.CHUNK_SIZE;
                    float topfym = heightfym - (float)chunk.WorldPosition.Z * Chunk.CHUNK_SIZE;
                    for (int z = Math.Max(top, 0); z < Math.Min(top + 1, Chunk.CHUNK_SIZE); z++)
                    {
                        //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z + z) / 10f) >= 0.5f;
                        ushort tsf = (ushort)(/*choice ? surf2 : */surf);
                        if (topf - top > 0f)
                        {
                            bool xp = topfxp > topf && topfxp - Math.Round(topfxp) <= 0;
                            bool xm = topfxm > topf && topfxm - Math.Round(topfxm) <= 0;
                            bool yp = topfyp > topf && topfyp - Math.Round(topfyp) <= 0;
                            bool ym = topfym > topf && topfym - Math.Round(topfym) <= 0;
                            if (xm && xp) { /* Fine as-is */ }
                            else if (ym && yp) { /* Fine as-is */ }
                            else if (yp && xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (yp && xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (xp && ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (xp && yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (ym && xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (ym && xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (xm && ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (xm && yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 0, 0, 0); } // TODO: Shape
                            else if (xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 80, 0, 0); }
                            else if (xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 81, 0, 0); }
                            else if (yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 82, 0, 0); }
                            else if (ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 83, 0, 0); }
                            else { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); }
                            if (z > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z - 1)] = new BlockInternal((ushort)(/*choice ? seco2 :*/ seco), 0, 0, 0);
                            }
                        }
                        else
                        {
                            bool xp = topfxp > topf && topfxp - Math.Round(topfxp) > 0;
                            bool xm = topfxm > topf && topfxm - Math.Round(topfxm) > 0;
                            bool yp = topfyp > topf && topfyp - Math.Round(topfyp) > 0;
                            bool ym = topfym > topf && topfym - Math.Round(topfym) > 0;
                            if (xm && xp) { /* Fine as-is */ }
                            else if (ym && yp) { /* Fine as-is */ }
                            else if (yp && xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (yp && xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (xp && ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (xp && yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (ym && xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (ym && xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (xm && ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (xm && yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 3, 0, 0); } // TODO: Shape
                            else if (xp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 73, 0, 0); }
                            else if (xm) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 72, 0, 0); }
                            else if (yp) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 74, 0, 0); }
                            else if (ym) { chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(tsf, 75, 0, 0); }
                            else { /* Fine as-is */ }
                        }
                    }
                    // Water
                    int level = 0 - (int)(chunk.WorldPosition.Z * Chunk.CHUNK_SIZE);
                    if (hheightint <= 0)
                    {
                       // bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z) / 10f) >= 0.5f;
                        ushort sandmat = (ushort)(/*choice ? biome2 : */biome).SandMaterial();
                        for (int z = Math.Max(top, 0); z < Math.Min(top + 1, Chunk.CHUNK_SIZE); z++)
                        {
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal(sandmat, 0, 0, 0);
                        }
                        for (int z = Math.Max(top + 1, 0); z <= Math.Min(level, Chunk.CHUNK_SIZE - 1); z++)
                        {
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)water, 0, (byte)Colors.M_BLUR, 0);
                        }
                    }
                    else
                    {
                        if (level >= 0 && level < Chunk.CHUNK_SIZE)
                        {
                            if (Math.Round(heightfxp) <= 0 || Math.Round(heightfxm) <= 0 || Math.Round(heightfyp) <= 0 || Math.Round(heightfym) <= 0)
                            {
                                //bool choice = SimplexNoise.Generate(cx / 10f, cy / 10f, ((float)cpos.Z) / 10f) >= 0.5f;
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, level)] = new BlockInternal((ushort)(/*choice ? biome2 : */biome).SandMaterial(), 0, 0, 0);
                            }
                        }
                    }
                    // Special case: trees.
                    if (hheight > 0 && top >= 0 && top < Chunk.CHUNK_SIZE)
                    {
#if TIMINGS
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
#endif
                        Random spotr = new Random((int)(SimplexNoise.Generate(seed2 + cx, Seed + cy) * 1000 * 1000)); // TODO: Improve!
                        if (spotr.Next(65) == 1) // TODO: Efficiency! // TODO: Biome based chance!
                        {
                            // TODO: Different plants per biome!
                            chunk.OwningRegion.SpawnSmallPlant("tallgrass", new Location(cx + 0.5f, cy + 0.5f, hheight), chunk);
                        }
                        else if (spotr.Next(300) == 1) // TODO: Efficiency! // TODO: Biome based chance!
                        {
                            // TODO: Different trees per biome!
                            chunk.OwningRegion.SpawnTree("treevox0" + (Utilities.UtilRandom.Next(2) + 1), new Location(cx + 0.5f, cy + 0.5f, hheight), chunk);
                        }
#if TIMINGS
                        sw.Stop();
                        Timings_Entities += sw.ElapsedTicks / (double)Stopwatch.Frequency;
#endif
                    }
                }
            }
        }
    }
}
