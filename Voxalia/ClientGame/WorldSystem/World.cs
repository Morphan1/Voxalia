using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.WorldSystem
{
    public class World
    {
        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();

        public Client TheClient;

        public Chunk GetChunk(Location pos)
        {
            pos.X = (int)pos.X - (int)pos.X % 30;
            pos.Y = (int)pos.Y - (int)pos.Y % 30;
            pos.Z = (int)pos.Z - (int)pos.Z % 30;
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                return chunk;
            }
            chunk = new Chunk();
            chunk.OwningWorld = this;
            chunk.WorldPosition = pos;
            LoadedChunks.Add(pos, chunk);
            return chunk;
        }
    }
}
