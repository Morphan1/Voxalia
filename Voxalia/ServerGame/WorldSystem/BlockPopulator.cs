using System;
using System.Collections.Generic;

namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BlockPopulator
    {
        public abstract float GetHeight(int seed, int seed2, int seed3, int seed4, int seed5, float x, float y, float z, out Biome biome);

        public abstract void Populate(int seed, int seed2, int seed3, int seed4, int seed5, Chunk chunk);

        public abstract List<Tuple<string, double>> GetTimings();

        public abstract void ClearTimings();
    }
}
