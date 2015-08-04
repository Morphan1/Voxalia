using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public class SimpleGeneratorCore: BlockPopulator
    {
        public const float GlobalHeightMapSize = 2000;
        public const float LocalHeightMapSize = 400;

        public override float GetHeight(short Seed, short seed2, float x, float y)
        {
            float lheight = SimplexNoise.Generate((float)seed2 + (x / GlobalHeightMapSize), (float)Seed + (y / GlobalHeightMapSize)) * 50f - 10f;
            float height = SimplexNoise.Generate((float)Seed + (x / LocalHeightMapSize), (float)seed2 + (y / LocalHeightMapSize)) * 6f - 3f;
            return lheight + height;
        }

        public override void Populate(short Seed, short seed2, Chunk chunk)
        {
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    float cx = (float)chunk.WorldPosition.X * 30 + x;
                    float cy = (float)chunk.WorldPosition.Y * 30 + y;
                    float hheight = GetHeight(Seed, seed2, cx, cy);
                    float topf = hheight - (float)chunk.WorldPosition.Z * 30;
                    int top = (int)Math.Round(topf);
                    for (int z = 0; z < Math.Min(top - 5, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.STONE, 0, 0));
                    }
                    for (int z = Math.Max(top - 5, 0); z < Math.Min(top - 1, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.DIRT, 0, 0));
                    }
                    for (int z = Math.Max(top - 1, 0); z < Math.Min(top, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 0, 0));
                    }
                    for (int z = Math.Max(top, 0); z < 30; z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.AIR, 0, 0));
                    }
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
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 80, 0));
                            }
                            else if (topfxm > topf && topfxm - Math.Round(topfxm) <= 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 81, 0));
                            }
                            else if (topfyp > topf && topfyp - Math.Round(topfyp) <= 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 82, 0));
                            }
                            else if (topfym > topf && topfym - Math.Round(topfym) <= 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 83, 0));
                            }
                            else
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 3, 0));
                            }
                            if (z > 0)
                            {
                                chunk.SetBlockAt(x, y, z - 1, new BlockInternal((ushort)Material.DIRT, 0, 0));
                            }
                        }
                        else
                        {
                            if (topfxp > topf && topfxp - Math.Round(topfxp) > 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 73, 0));
                            }
                            else if (topfxm > topf && topfxm - Math.Round(topfxm) > 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 72, 0));
                            }
                            else if (topfyp > topf && topfyp - Math.Round(topfyp) > 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 74, 0));
                            }
                            else if (topfym > topf && topfym - Math.Round(topfym) > 0)
                            {
                                chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 75, 0));
                            }
                        }
                    }
                }
            }
        }
    }
}
