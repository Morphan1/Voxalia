using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleLakeBiome : SimpleBiome
    {
        public override string GetName()
        {
            return "Lake";
        }

        public override Material SurfaceBlock()
        {
            return Material.SAND;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SANDSTONE;
        }
    }
}
