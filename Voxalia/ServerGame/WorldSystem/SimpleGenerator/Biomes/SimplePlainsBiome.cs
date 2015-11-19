using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimplePlainsBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Plains";
        }

        public override Material SurfaceBlock()
        {
            return Material.GRASS_PLAINS;
        }

        public override float HeightMod()
        {
            return 0.2f;
        }
    }
}
