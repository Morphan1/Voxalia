using Voxalia.Shared;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public class SimpleBiomeGenerator: BiomeGenerator
    {
        public double TemperatureMapSize = 2400;

        public double DownfallMapSize = 4800;

        public override double GetTemperature(int seed2, int seed3, double x, double y)
        {
            return SimplexNoise.Generate((double)seed2 + (x / TemperatureMapSize), (double)seed3 + (y / TemperatureMapSize)) * 100f;
        }

        public override double GetDownfallRate(int seed3, int seed4, double x, double y)
        {
            return SimplexNoise.Generate((double)seed3 + (x / DownfallMapSize), (double)seed4 + (y / DownfallMapSize));
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

        SimpleStoneBiome Stone = new SimpleStoneBiome();

        public override Biome BiomeFor(int seed2, int seed3, int seed4, double x, double y, double z, double height)
        {
            if (z < -300)
            {
                return Hell;
            }
            if (z < 90)
            {
                return Stone;
            }
            double temp = GetTemperature(seed2, seed3, x, y);
            double down = GetDownfallRate(seed3, seed4, x, y);
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
                // TODO: Snow hill, etc?
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
    }
}
