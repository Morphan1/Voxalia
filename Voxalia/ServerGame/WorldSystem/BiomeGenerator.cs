namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BiomeGenerator
    {
        public abstract float GetTemperature(int seed2, int seed3, float x, float y);

        public abstract float GetDownfallRate(int seed3, int seed4, float x, float y);

        public abstract Biome BiomeFor(int seed2, int seed3, int seed4, float x, float y, float z, float height);
    }
}
