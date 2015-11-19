using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleSnowBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Snow";
        }

        public override Material SurfaceBlock()
        {
            return Material.SNOW;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SNOW;
        }
    }
}
