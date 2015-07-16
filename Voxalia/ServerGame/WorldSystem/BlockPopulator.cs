using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.WorldSystem
{
    public abstract class BlockPopulator
    {
        public abstract void Populate(short seed, short seed2, Chunk chunk);
    }
}
