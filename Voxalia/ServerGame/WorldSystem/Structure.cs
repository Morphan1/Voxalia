using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Structure
    {
        public Vector3i Size;

        public BlockInternal[] Blocks;

        public Vector3i Origin;

        public int BlockIndex(int x, int y, int z)
        {
            return z * Size.Y * Size.X + y * Size.X + x;
        }

        Location[] FloodDirs = new Location[] { Location.UnitX, Location.UnitY, -Location.UnitX, -Location.UnitY, Location.UnitZ, -Location.UnitZ };

        // TODO: Optimize tracing!
        public Structure(Region tregion, Location startOfTrace, int maxrad)
        {
            startOfTrace = startOfTrace.GetBlockLocation();
            Queue<Location> locs = new Queue<Location>();
            HashSet<Location> found = new HashSet<Location>();
            List<Location> resultLocs = new List<Location>();
            locs.Enqueue(startOfTrace);
            int maxradsq = maxrad * maxrad;
            AABB box = new AABB() { Max = startOfTrace, Min = startOfTrace };
            while (locs.Count > 0)
            {
                Location loc = locs.Dequeue();
                if (found.Contains(loc))
                {
                    continue;
                }
                if ((loc - startOfTrace).LengthSquared() > maxradsq)
                {
                    throw new Exception("Escaped radius!");
                }
                BlockInternal bi = tregion.GetBlockInternal(loc);
                if ((Material)bi.BlockMaterial == Material.AIR)
                {
                    continue;
                }
                if (!((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.EDITED))
                {
                    throw new Exception("Found natural block!");
                }
                if (((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.PROTECTED))
                {
                    throw new Exception("Found protected block!");
                }
                found.Add(loc);
                resultLocs.Add(loc);
                box.Include(loc);
                foreach (Location dir in FloodDirs)
                {
                    locs.Enqueue(loc + dir);
                }
            }
            Location ext = box.Max - box.Min;
            Size = new Vector3i((int)ext.X + 1, (int)ext.Y + 1, (int)ext.Z + 1);
            Origin = new Vector3i((int)Math.Floor(startOfTrace.X - box.Min.X), (int)Math.Floor(startOfTrace.Y - box.Min.Y), (int)Math.Floor(startOfTrace.Z - box.Min.Z));
            Blocks = new BlockInternal[Size.X * Size.Y * Size.Z];
            foreach (Location loc in resultLocs)
            {
                Blocks[BlockIndex((int)(loc.X - box.Min.X), (int)(loc.Y - box.Min.Y), (int)(loc.Z - box.Min.Z))] = tregion.GetBlockInternal(loc);
            }
        }

        public Structure(byte[] dat)
        {
            Size.X = Utilities.BytesToInt(Utilities.BytesPartial(dat, 0, 4));
            Size.Y = Utilities.BytesToInt(Utilities.BytesPartial(dat, 4, 4));
            Size.Z = Utilities.BytesToInt(Utilities.BytesPartial(dat, 8, 4));
            Origin.X = Utilities.BytesToInt(Utilities.BytesPartial(dat, 12, 4));
            Origin.Y = Utilities.BytesToInt(Utilities.BytesPartial(dat, 12 + 4, 4));
            Origin.Z = Utilities.BytesToInt(Utilities.BytesPartial(dat, 12 + 8, 4));
            Blocks = new BlockInternal[Size.X * Size.Y * Size.Z];
            for (int i = 0; i < Blocks.Length; i++)
            {
                Blocks[i] = new BlockInternal(Utilities.BytesToUshort(Utilities.BytesPartial(dat, 12 + 12 + i * 2, 2)), dat[12 + 12 + Blocks.Length * 2 + i], dat[12 + 12 + Blocks.Length * 3 + i]);
            }
        }

        public byte[] ToBytes()
        {
            byte[] dat = new byte[12 + 12 + Blocks.Length * 4];
            Utilities.IntToBytes(Size.X).CopyTo(dat, 0);
            Utilities.IntToBytes(Size.Y).CopyTo(dat, 4);
            Utilities.IntToBytes(Size.Z).CopyTo(dat, 8);
            Utilities.IntToBytes(Origin.X).CopyTo(dat, 12);
            Utilities.IntToBytes(Origin.Y).CopyTo(dat, 12 + 4);
            Utilities.IntToBytes(Origin.Z).CopyTo(dat, 12 + 8);
            for (int i = 0; i < Blocks.Length; i++)
            {
                Utilities.UshortToBytes(Blocks[i].BlockMaterial).CopyTo(dat, 12 + 12 + i * 2);
                dat[12 + 12 + Blocks.Length * 2 + i] = Blocks[i].BlockData;
                dat[12 + 12 + Blocks.Length * 3 + i] = Blocks[i].BlockLocalData;
            }
            return dat;
        }

        public void Paste(Region tregion, Location corner)
        {
            corner.X -= Origin.X;
            corner.Y -= Origin.Y;
            corner.Z -= Origin.Z;
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        BlockInternal bi = Blocks[BlockIndex(x, y, z)];
                        if ((Material)bi.BlockMaterial != Material.AIR)
                        {
                            bi.BlockLocalData = (byte)(bi.BlockLocalData | ((int)BlockFlags.EDITED));
                            tregion.SetBlockMaterial(corner + new Location(x, y, z), (Material)bi.BlockMaterial, bi.BlockData, (byte)(bi.BlockLocalData | (byte)BlockFlags.EDITED));
                        }
                    }
                }
            }
        }

        public void PasteCustom(Region tregion, Location corner)
        {
            corner.X -= Origin.X;
            corner.Y -= Origin.Y;
            corner.Z -= Origin.Z;
            for (int x = 0; x < Size.X; x++)
            {
                for (int y = 0; y < Size.Y; y++)
                {
                    for (int z = 0; z < Size.Z; z++)
                    {
                        BlockInternal bi = Blocks[BlockIndex(x, y, z)];
                        if ((Material)bi.BlockMaterial != Material.AIR)
                        {
                            bi.BlockLocalData = (byte)(bi.BlockLocalData & ~((int)BlockFlags.EDITED));
                            Location forpos = new Location(corner.X + x, corner.Y + y, corner.Z + z);
                            Location chunkpos = tregion.ChunkLocFor(forpos);
                            Chunk ch = tregion.LoadChunkNoPopulate(chunkpos);
                            ch.SetBlockAt((int)(forpos.X - chunkpos.X * Chunk.CHUNK_SIZE), (int)(forpos.Y - chunkpos.Y * Chunk.CHUNK_SIZE), (int)(forpos.Z - chunkpos.Z * Chunk.CHUNK_SIZE), bi);
                        }
                    }
                }
            }
        }
    }
}
