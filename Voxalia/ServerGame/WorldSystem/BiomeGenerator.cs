using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BiomeGenerator
    {
        public abstract float GetTemperature(short seed, short seed2, float x, float y);

        public abstract float GetDownfallRate(short seed, short seed2, float x, float y);

        public abstract Biome BiomeFor(short seed, short seed2, float x, float y, float z, float height);
    }
}
