using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public class SimpleBiomeGenerator: BiomeGenerator
    {
        public float TemperatureMapSize = 1200;

        public float DownfallMapSize = 2400;

        public override float GetTemperature(short seed, short seed2, float x, float y)
        {
            return SimplexNoise.Generate((float)seed + (x / TemperatureMapSize), (float)seed + (y / TemperatureMapSize)) * 100f;
        }

        public override float GetDownfallRate(short seed, short seed2, float x, float y)
        {
            return SimplexNoise.Generate((float)seed2 + (x / DownfallMapSize), (float)seed2 + (y / DownfallMapSize));
        }

        public SimpleRainForestBiome RainForest = new SimpleRainForestBiome();

        public SimpleForestBiome Forest = new SimpleForestBiome();

        public SimpleSwampBiome Swamp = new SimpleSwampBiome();

        public SimplePlainsBiome Plains = new SimplePlainsBiome();

        public SimpleDesertBiome Desert = new SimpleDesertBiome();

        public SimpleIcyBiome Icy = new SimpleIcyBiome();

        public SimpleSnowBiome Snow = new SimpleSnowBiome();

        SimpleLightForestHillBiome LightForestHill = new SimpleLightForestHillBiome();

        SimpleMountainBiome Mountain = new SimpleMountainBiome();

        SimpleColdMountainBiome ColdMountain = new SimpleColdMountainBiome();

        SimpleLakeBiome Lake = new SimpleLakeBiome();

        SimpleFrozenLakeBiome FrozenLake = new SimpleFrozenLakeBiome();

        SimpleHellBiome Hell = new SimpleHellBiome();

        public override Biome BiomeFor(short seed, short seed2, float x, float y, float z, float height)
        {
            if (z > -50)
            {
                float temp = GetTemperature(seed, seed2, x, y);
                float down = GetDownfallRate(seed, seed2, x, y);
                if (height > 0f && height < 20f)
                {
                    if (down >= 0.8f && temp >= 80f)
                    {
                        return RainForest;
                    }
                    if (down >= 0.5f && down < 0.8f && temp >= 60f)
                    {
                        return Forest;
                    }
                    else if (down >= 0.3f && down < 0.5f && temp >= 90f)
                    {
                        return Swamp;
                    }
                    if (down >= 0.3f && down < 0.5f && temp >= 50f && temp < 90f)
                    {
                        return Plains;
                    }
                    if (down < 0.3f && temp >= 50f)
                    {
                        return Desert;
                    }
                    if (temp >= 32f)
                    {
                        return Plains;
                    }
                    if (down > 0.5f)
                    {
                        return Snow;
                    }
                    else
                    {
                        return Icy;
                    }
                }
                else if (height >= 20 && height < 40)
                {
                    return LightForestHill;
                }
                else if (height >= 40)
                {
                    if (temp > 70)
                    {
                        return Mountain;
                    }
                    else
                    {
                        return ColdMountain;
                    }
                }
                else
                {
                    if (temp > 32)
                    {
                        return Lake;
                    }
                    else
                    {
                        return FrozenLake;
                    }
                }
            }
            else
            {
                return Hell;
            }
        }
    }
}
