//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BiomeGenerator
    {
        public abstract double GetTemperature(int seed2, int seed3, double x, double y);

        public abstract double GetDownfallRate(int seed3, int seed4, double x, double y);

        public abstract Biome BiomeFor(int seed2, int seed3, int seed4, double x, double y, double z, double height);
    }
}
