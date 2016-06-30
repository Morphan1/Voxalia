using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleHellBiome: SimpleBiome
    {
        public override string GetName()
        {
            return "Hell";
        }

        public override Material BaseBlock()
        {
            return Material.HELLSTONE;
        }

        public override float AirDensity()
        {
            return 0.5f;
        }
    }
}
