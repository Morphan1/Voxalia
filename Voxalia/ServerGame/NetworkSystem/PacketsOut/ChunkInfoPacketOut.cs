using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class ChunkInfoPacketOut: AbstractPacketOut
    {
        public ChunkInfoPacketOut(Chunk chunk, int lod)
        {
            ID = 24;
            byte[] data_orig;
            if (lod == 1)
            {
                data_orig = new byte[chunk.BlocksInternal.Length * 3];
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    Utilities.UshortToBytes(chunk.BlocksInternal[i].BlockMaterial).CopyTo(data_orig, i * 2);
                }
                for (int i = 0; i < chunk.BlocksInternal.Length; i++)
                {
                    data_orig[chunk.BlocksInternal.Length * 2 + i] = chunk.BlocksInternal[i].BlockData;
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
