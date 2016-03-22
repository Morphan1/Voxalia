using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Voxalia.ServerGame.WorldSystem
{
    public class ChunkDetails
    {
        public byte[] Blocks;
        public byte[] Entities;
        public ChunkFlags Flags;
        public int X, Y, Z;
        public int Version;
    }
}
