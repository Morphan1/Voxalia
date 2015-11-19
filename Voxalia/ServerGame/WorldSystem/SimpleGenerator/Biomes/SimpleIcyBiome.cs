using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleIcyBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Icy";
        }

        public override Material SurfaceBlock()
        {
            return Material.SNOW; // TODO: Ice?
        }

        public override Material SecondLayerBlock()
        {
            return Material.SNOW;
        }
    }
}
