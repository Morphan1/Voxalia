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
        
        public virtual float HeightMod()
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
    }
}
