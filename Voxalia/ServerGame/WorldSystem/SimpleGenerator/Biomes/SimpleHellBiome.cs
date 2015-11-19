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
            return base.BaseBlock(); // TODO: Hell stone!
        }
    }
}
