using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator.Biomes
{
    public class SimpleAlmostHellBiome : SimpleBiome
    {
        public override string GetName()
        {
            return "AlmostHell";
        }

        public override double AirDensity()
        {
            return 0.7f;
        }
    }
}
