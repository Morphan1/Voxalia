//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem.SimpleGenerator
{
    public abstract class SimpleBiome: Biome
    {
        public virtual Material SurfaceBlock()
        {
            return Material.GRASS_FOREST;
        }

        public virtual Material SecondLayerBlock()
        {
            return Material.DIRT;
        }

        public virtual Material BaseBlock()
        {
            return Material.STONE;
        }
        
        public virtual double HeightMod()
        {
            return 1;
        }

        public virtual Material WaterMaterial()
        {
            return Material.DIRTY_WATER;
        }

        public virtual Material SandMaterial()
        {
            return Material.SAND;
        }

        public virtual double AirDensity()
        {
            return 0.8f;
        }
    }
}
