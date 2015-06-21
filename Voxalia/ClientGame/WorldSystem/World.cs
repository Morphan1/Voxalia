using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.WorldSystem
{
    public class World
    {
        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();
    }
}
