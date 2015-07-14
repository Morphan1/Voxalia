using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem
{
    public class QuickGenerator: BlockPopulator
    {
        public override void Populate(long Seed, Chunk chunk)
        {
            for (int x = 0; x < 30; x++)
            {
                for (int y = 0; y < 30; y++)
                {
                    int height = (int)(SimplexNoise.Generate((float)(Seed + chunk.WorldPosition.X * 30 + x) / 90f, (float)(Seed + chunk.WorldPosition.Y * 30 + y) / 60f) * 6f - 3f);
                    int top = height - (int)chunk.WorldPosition.Z * 30;
                    for (int z = 0; z < Math.Min(top - 5, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.STONE, 0));
                    }
                    for (int z = Math.Max(top - 5, 0); z < Math.Min(top - 1, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.DIRT, 0));
                    }
                    for (int z = Math.Max(top - 1, 0); z < Math.Min(top, 30); z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.GRASS, 0));
                    }
                    for (int z = Math.Max(top, 0); z < 30; z++)
                    {
                        chunk.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.AIR, 0));
                    }
                }
            }
        }
    }
}
