using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    class SimpleColdMountainBiome : SimpleBiome
    {
        public override string GetName()
        {
            return "ColdMountain";
        }

        public override Material SurfaceBlock()
        {
            return Material.SNOW_SOLID;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SNOW_SOLID;
        }
    }
}
