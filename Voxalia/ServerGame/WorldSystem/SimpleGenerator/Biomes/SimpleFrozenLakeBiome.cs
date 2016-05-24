using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleFrozenLakeBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "FrozenLake";
        }

        public override Material SurfaceBlock()
        {
            return Material.SNOW_SOLID;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SNOW_SOLID;
        }

        public override Material SandMaterial()
        {
            return Material.SNOW_SOLID;
        }

        public override Material WaterMaterial()
        {
            return Material.ICE;
        }
    }
}
