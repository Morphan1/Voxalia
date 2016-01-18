using System;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ChunkInfoPacketOut: AbstractPacketOut
    {
        public ChunkInfoPacketOut(Chunk chunk, int lod)
        {
            if (chunk.Flags.HasFlag(ChunkFlags.POPULATING))
            {
                throw new Exception("Trying to transmit chunk while it's still loading! For chunk at " + chunk);
            }
            ID = 24;
            byte[] data_orig;
            if (lod == 1)
            {
                data_orig = new byte[chunk.BlocksInternal.Length * 4];
                int csize = Chunk.CHUNK_SIZE;
                for (int x = 0; x < csize; x++)
                {
                    for (int y = 0; y < csize; y++)
                    {
                        for (int z = 0; z < csize; z++)
                        {
                            Utilities.UshortToBytes(chunk.BlocksInternal[chunk.LODBlockIndex(x, y, z, lod)].BlockMaterial).CopyTo(data_orig, (z * csize * csize + y * csize + x) * 2);
                        }
                    }
                }
                // TODO: Why is the above and below different?
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    data_orig[chunk.BlocksInternal.Length * 2 + i] = chunk.BlocksInternal[i].BlockData;
                    data_orig[chunk.BlocksInternal.Length * 3 + i] = chunk.BlocksInternal[i].BlockPaint;
                }
            }
            else
            {
                int csize = Chunk.CHUNK_SIZE / lod;
                data_orig = new byte[csize * csize * csize * 2];
                for (int x = 0; x < csize; x++)
                {
                    for (int y = 0; y < csize; y++)
                    {
                        for (int z = 0; z < csize; z++)
                        {
                            Utilities.UshortToBytes(chunk.BlocksInternal[chunk.LODBlockIndex(x, y, z, lod)].BlockMaterial).CopyTo(data_orig, (z * csize * csize + y * csize + x) * 2);
                        }
                    }
                }
            }
            byte[] gdata = FileHandler.GZip(data_orig);
            DataStream ds = new DataStream(gdata.Length + 16);
            DataWriter dw = new DataWriter(ds);
            dw.WriteInt((int)chunk.WorldPosition.X);
            dw.WriteInt((int)chunk.WorldPosition.Y);
            dw.WriteInt((int)chunk.WorldPosition.Z);
            dw.WriteInt(lod);
            dw.WriteBytes(gdata);
            Data = ds.ToArray();
        }
    }
}
