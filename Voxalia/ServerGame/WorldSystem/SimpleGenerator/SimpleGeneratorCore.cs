using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public class SimpleGeneratorCore: BlockPopulator
    {
        public const float GlobalHeightMapSize = 1500;
        public const float LocalHeightMapSize = 40;

        public override float GetHeight(short Seed, short seed2, float x, float y)
        {
            float lheight = SimplexNoise.Generate((float)seed2 + (x / GlobalHeightMapSize), (float)Seed + (y / GlobalHeightMapSize)) * 50f - 10f;
            float height = SimplexNoise.Generate((float)Seed + (x / LocalHeightMapSize), (float)seed2 + (y / LocalHeightMapSize)) * 6f - 3f;
            return lheight + height;
        }
        
        void SpecialSetBlockAt(Chunk chunk, int X, int Y, int Z, BlockInternal bi)
        {
            if (X < 0 || Y < 0 || Z < 0 || X >= Chunk.CHUNK_SIZE || Y >= Chunk.CHUNK_SIZE || Z >= Chunk.CHUNK_SIZE)
            {
                Location chloc = chunk.OwningRegion.ChunkLocFor(new Location(X, Y, Z));
                Chunk ch = chunk.OwningRegion.LoadChunkNoPopulate(chunk.WorldPosition + chloc);
                int x = (int)(X - chloc.X * Chunk.CHUNK_SIZE);
                int y = (int)(Y - chloc.Y * Chunk.CHUNK_SIZE);
                int z = (int)(Z - chloc.Z * Chunk.CHUNK_SIZE);
                if (x < 0 || y < 0 || z < 0 || x >= Chunk.CHUNK_SIZE || y >= Chunk.CHUNK_SIZE || z >= Chunk.CHUNK_SIZE)
                {
                    SysConsole.Output(OutputType.WARNING, "Invalid: " + x + ", " + y + ", " + z + " from " + X +", " + Y + ", " + Z + ", " + chloc);
                }
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

        public override void Populate(short Seed, short seed2, Chunk chunk)
        {
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    // Prepare basics
                    float cx = (float)chunk.WorldPosition.X * 30f + x;
                    float cy = (float)chunk.WorldPosition.Y * 30f + y;
                    float hheight = GetHeight(Seed, seed2, cx, cy);
                    float topf = hheight - (float)chunk.WorldPosition.Z * 30;
                    int top = (int)Math.Round(topf);
                    // General natural ground
                    for (int z = 0; z < Math.Min(top - 5, 30); z++)
                    {
                        chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.STONE, 0, 0);
                    }
                    for (int z = Math.Max(top - 5, 0); z < Math.Min(top - 1, 30); z++)
                    {
                        chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.DIRT, 0, 0);
                    }
                    for (int z = Math.Max(top - 1, 0); z < Math.Min(top, 30); z++)
                    {
                        chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 0, 0);
                    };
                    if (top >= 0 && top < 30)
                    {
                        Random spotr = new Random((int)(SimplexNoise.Generate(seed2 + cx, Seed + cy) * 1500 * 1500));
                        int n = spotr.Next(50);
                        if (n == 1)
                        {
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, top)] = new BlockInternal((ushort)Material.TALLGRASS, 52, 0);
                        }
                        else if (n == 2)
                        {
                            chunk.BlocksInternal[chunk.BlockIndex(x, y, top)] = new BlockInternal((ushort)Material.TALLGRASS, 127, 0);
                        }
                    }
                    // Smooth terrain cap
                    for (int z = Math.Max(top, 0); z < Math.Min(top + 1, 30); z++)
                    {
                        float topfxp = GetHeight(Seed, seed2, cx + 1, cy) - (float)chunk.WorldPosition.Z * 30;
                        float topfxm = GetHeight(Seed, seed2, cx - 1, cy) - (float)chunk.WorldPosition.Z * 30;
                        float topfyp = GetHeight(Seed, seed2, cx, cy + 1) - (float)chunk.WorldPosition.Z * 30;
                        float topfym = GetHeight(Seed, seed2, cx, cy - 1) - (float)chunk.WorldPosition.Z * 30;
                        if (topf - top > 0f)
                        {
                            if (topfxp > topf && topfxp - Math.Round(topfxp) <= 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 80, 0);
                            }
                            else if (topfxm > topf && topfxm - Math.Round(topfxm) <= 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 81, 0);
                            }
                            else if (topfyp > topf && topfyp - Math.Round(topfyp) <= 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 82, 0);
                            }
                            else if (topfym > topf && topfym - Math.Round(topfym) <= 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 83, 0);
                            }
                            else
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 3, 0);
                            }
                            if (z > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z - 1)] = new BlockInternal((ushort)Material.DIRT, 0, 0);
                            }
                        }
                        else
                        {
                            if (topfxp > topf && topfxp - Math.Round(topfxp) > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 73, 0);
                            }
                            else if (topfxm > topf && topfxm - Math.Round(topfxm) > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 72, 0);
                            }
                            else if (topfyp > topf && topfyp - Math.Round(topfyp) > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 74, 0);
                            }
                            else if (topfym > topf && topfym - Math.Round(topfym) > 0)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.GRASS, 75, 0);
                            }
                        }
                    }
                    // Special case: trees.
                    // TODO: Separate generator?
                    if (top >= -7 && top < 30)
                    {
                        Random spotr = new Random((int)(SimplexNoise.Generate(seed2 + cx, Seed + cy) * 1000 * 1000));
                        if (spotr.Next(75) == 1) // TODO: Efficiency!
                        {
                            int cap = Math.Min(top + 5, 30);
                            for (int z = Math.Max(top, 0); z < cap; z++)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.LOG, 39, 0);
                            }
                            cap = Math.Min(top + 7, 30);
                            for (int z = Math.Max(top + 5, 0); z < cap; z++)
                            {
                                chunk.BlocksInternal[chunk.BlockIndex(x, y, z)] = new BlockInternal((ushort)Material.LEAVES1, 0, 0);
                            }
                            cap = Math.Min(top + 7, 30);
                            for (int z = Math.Max(top + 3, 0); z < cap; z++)
                            {
                                int width = 2;
                                if (z == top + 3 || z == top + 6)
                                {
                                    width = 1;
                                }
                                int xcap = x + 1 + width;
                                for (int sx = x - width; sx < xcap; sx++)
                                {
                                    int ycap = y + 1 + width;
                                    for (int sy = y - width; sy < ycap; sy++)
                                    {
                                        if (sy != y || sx != x)
                                        {
                                            SpecialSetBlockAt(chunk, sx, sy, z, new BlockInternal((ushort)Material.LEAVES1, 0, 0));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
