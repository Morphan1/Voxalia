using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.WorldSystem
{
    public class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public World OwningWorld = null;

        public Location WorldPosition;

        public ushort[] BlocksInternal = new ushort[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
    }
}
