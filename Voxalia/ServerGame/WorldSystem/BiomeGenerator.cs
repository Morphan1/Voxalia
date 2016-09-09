namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BiomeGenerator
    {
        public abstract double GetTemperature(int seed2, int seed3, double x, double y);

        public abstract double GetDownfallRate(int seed3, int seed4, double x, double y);

        public abstract Biome BiomeFor(int seed2, int seed3, int seed4, double x, double y, double z, double height);
    }
}
