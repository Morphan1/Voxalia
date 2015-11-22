using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleDesertBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Desert";
        }

        public override Material SurfaceBlock()
        {
            return Material.SAND;
        }

        public override Material SecondLayerBlock()
        {
            return Material.SAND;
        }

        public override Material BaseBlock()
        {
            return base.BaseBlock();
        }

        public override Material TallGrassBlock()
        {
            return Material.AIR; // TODO: Desert plants!
        }
    }
}
