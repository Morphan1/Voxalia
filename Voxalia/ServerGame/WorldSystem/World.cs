using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.WorldSystem
{
    public class World
    {
        public string Name = null;

        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();

        public Chunk LoadChunk(Location pos)
        {
            pos.X = (int)pos.X - (int)pos.X % 30;
            pos.Y = (int)pos.Y - (int)pos.Y % 30;
            pos.Z = (int)pos.Z - (int)pos.Z % 30;
            Chunk chunk;
            if (LoadedChunks.TryGetValue(pos, out chunk))
            {
                return chunk;
            }
            // TODO: Actually load from file
            chunk = new Chunk();
            PopulateChunk(chunk);
            LoadedChunks.Add(pos, chunk);
            return chunk;
        }

        public void PopulateChunk(Chunk chunk)
        {
            if (chunk.WorldPosition.X < 0)
            {
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    chunk.BlocksInternal[i] = (ushort)Material.STONE;
                }
            }
            else
            {
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    chunk.BlocksInternal[i] = (ushort)Material.AIR;
                }
            }
        }
    }
}
