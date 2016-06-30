using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.WorldSystem
{
    public class ChunkDetails
    {
        public byte[] Blocks;
        public ChunkFlags Flags;
        public int X, Y, Z;
        public int Version;
        public byte[] Reachables;
    }
}
