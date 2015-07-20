using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class ChunkInfoPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            TheClient.Schedule.StartASyncTask(() => ParseData(data));
            return true;
        }

        void ParseData(byte[] data)
        {
            DataStream ds = new DataStream(data);
            DataReader dr = new DataReader(ds);
            int x = dr.ReadInt();
            int y = dr.ReadInt();
            int z = dr.ReadInt();
            int posMult = dr.ReadInt();
            int csize = Chunk.CHUNK_SIZE / posMult;
            byte[] data_unzipped = dr.ReadBytes(data.Length - 16);
            byte[] data_orig = FileHandler.UnGZip(data_unzipped);
            if (posMult == 1)
            {
                if (data_orig.Length != Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 3)
                {
                    SysConsole.Output(OutputType.WARNING, "Invalid chunk size!" + (Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 3) + ", got " + data_orig.Length + ")");
                    return;
                }
            }
            else if (data_orig.Length != csize * csize * csize * 2)
            {
                SysConsole.Output(OutputType.WARNING, "Invalid LOD'ed chunk size! (LOD = " + posMult + ", Expected " + (csize * csize * csize * 2) + ", got " + data_orig.Length + ")");
                return;
            }
            TheClient.Schedule.ScheduleSyncTask(() =>
            {
                Chunk chk = TheClient.TheWorld.LoadChunk(new Location(x, y, z), posMult);
                TheClient.Schedule.StartASyncTask(() => parsechunk2(chk, data_orig, posMult));
            });
        }

        void parsechunk2(Chunk chk, byte[] data_orig, int posMult)
        {
            for (int x = 0; x < chk.CSize; x++)
            {
                for (int y = 0; y < chk.CSize; y++)
                {
                    for (int z = 0; z < chk.CSize; z++)
                    {
                        chk.BlocksInternal[chk.BlockIndex(x, y, z)].BlockMaterial = Utilities.BytesToUshort(Utilities.BytesPartial(data_orig, (z * chk.CSize * chk.CSize + y * chk.CSize + x) * 2, 2));
                    }
                }
            }
            if (posMult == 1)
            {
                for (int i = 0; i < chk.BlocksInternal.Length; i++)
                {
                    chk.BlocksInternal[i].BlockData = data_orig[chk.BlocksInternal.Length * 2 + i];
                }
            }
            else
            {
                for (int i = 0; i < chk.BlocksInternal.Length; i++)
                {
                    chk.BlocksInternal[i].BlockData = 0;
                }
            }
            TheClient.Schedule.ScheduleSyncTask(() => { chk.AddToWorld(); chk.CreateVBO(); chk.UpdateSurroundingsFully(); });
        }
    }
}
