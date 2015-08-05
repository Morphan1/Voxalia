using System;
using System.Collections.Generic;
using System.Text;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public bool LOADING = true;

        public Region OwningRegion = null;

        public Location WorldPosition;

        public Chunk()
        {
            BlocksInternal = new BlockInternal[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        }

        public BlockInternal[] BlocksInternal;

        /// <summary>
        /// Asyncable (math only).
        /// </summary>
        public int LODBlockIndex(int x, int y, int z, int lod)
        {
            int cs = CHUNK_SIZE / lod;
            return z * CHUNK_SIZE * CHUNK_SIZE * lod + y * CHUNK_SIZE * lod + x * lod;
        }

        /// <summary>
        /// Asyncable (math only).
        /// </summary>
        public int BlockIndex(int x, int y, int z)
        {
            return z * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + x;
        }

        /// <summary>
        /// Asyncable (Edit session).
        /// TODO: Edit session lock notes.
        /// </summary>
        public void SetBlockAt(int x, int y, int z, BlockInternal mat)
        {
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }

        public double LastEdited = -1;

        /// <summary>
        /// Asyncable (Edit session).
        /// TODO: Edit session lock notes.
        /// </summary>
        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }

        /// <summary>
        /// Asyncable (Edit session).
        /// TODO: Local edit session lock.
        /// </summary>
        /// <returns></returns>
        public StaticMesh CalculateChunkShape()
        {
            List<Vector3> Vertices = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6); // TODO: Make this an array?
            Vector3 ppos = WorldPosition.ToBVector() * 30;
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        if (((Material)c.BlockMaterial).IsSolid())
                        {
                            BlockInternal zp = z + 1 < CHUNK_SIZE ? GetBlockAt(x, y, z + 1) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(x, y, 30));
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(x, y, -1));
                            BlockInternal yp = y + 1 < CHUNK_SIZE ? GetBlockAt(x, y + 1, z) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(x, 30, z));
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(x, -1, z));
                            BlockInternal xp = x + 1 < CHUNK_SIZE ? GetBlockAt(x + 1, y, z) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(30, y, z));
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : OwningRegion.GetBlockInternal_NoLoad(Location.FromBVector(ppos) + new Location(-1, y, z));
                            bool zps = ((Material)zp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[zp.BlockData].OccupiesTOP();
                            bool zms = ((Material)zm.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[zm.BlockData].OccupiesBOTTOM();
                            bool xps = ((Material)xp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXP();
                            bool xms = ((Material)xm.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXM();
                            bool yps = ((Material)yp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYP();
                            bool yms = ((Material)ym.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYM();
                            Vector3 pos = new Vector3(ppos.X + x, ppos.Y + y, ppos.Z + z);
                            List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            Vertices.AddRange(vecsi);
                        }
                    }
                }
            }
            if (Vertices.Count == 0)
            {
                return null;
            }
            int[] inds = new int[Vertices.Count];
            for (int i = 0; i < Vertices.Count; i++)
            {
                inds[i] = i;
            }
            Vector3[] vecs = Vertices.ToArray();
            StaticMesh sm = new StaticMesh(vecs, inds);
            return sm;
        }

        StaticMesh worldObject = null;

        public ASyncScheduleItem adding = null;
        
        /// <summary>
        /// Sync only.
        /// Probably.
        /// TODO: Async? (Probably just a single local Add lock is sufficient).
        /// </summary>
        public void AddToWorld(Action callback = null)
        {
            if (adding != null)
            {
                ASyncScheduleItem item = OwningRegion.TheServer.Schedule.AddASyncTask(() => AddInternal(callback));
                adding = adding.ReplaceOrFollowWith(item);
            }
            else
            {
                adding = OwningRegion.TheServer.Schedule.StartASyncTask(() => AddInternal(callback));
            }
        }

        void AddInternal(Action callback)
        {
            StaticMesh tregionObject = CalculateChunkShape();
            OwningRegion.TheServer.Schedule.ScheduleSyncTask(() =>
            {
                if (worldObject != null)
                {
                    OwningRegion.RemoveChunkQuiet(worldObject);
                }
                worldObject = tregionObject;
                if (worldObject != null)
                {
                    OwningRegion.AddChunk(worldObject);
                }
                if (callback != null)
                {
                    callback.Invoke();
                }
            });
        }

        /// <summary>
        /// Asyncable.
        /// TODO: Local edit session lock.
        /// </summary>
        public byte[] GetSaveData()
        {
            byte[] bytes = new byte[8 + BlocksInternal.Length * 4];
            Encoding.ASCII.GetBytes("VOX_").CopyTo(bytes, 0); // General Header
            Utilities.IntToBytes(1).CopyTo(bytes, 4); // Saves Version
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                Utilities.UshortToBytes(BlocksInternal[i].BlockMaterial).CopyTo(bytes, 8 + i * 2);
                bytes[8 + BlocksInternal.Length * 2 + i] = BlocksInternal[i].BlockData;
                bytes[8 + BlocksInternal.Length * 3 + i] = BlocksInternal[i].BlockLocalData;
            }
            return FileHandler.GZip(bytes);
        }

        Object SaveLock = new Object();

        /// <summary>
        /// Sync only.
        /// Call OwningWorld.AddChunk(null) to recalculate after done.
        /// </summary>
        public void UnloadSafely(Action callback = null)
        {
            if (worldObject != null)
            {
                OwningRegion.RemoveChunkQuiet(worldObject);
                worldObject = null;
            }
            if (LastEdited >= 0)
            {
                SaveToFile(callback);
            }
            else
            {
                if (callback != null)
                {
                    callback.Invoke();
                }
            }
        }

        /// <summary>
        /// Asyncable (just launches async internals + 1 safe edit).
        /// </summary>
        public void SaveToFile(Action callback = null)
        {
            LastEdited = -1; // TODO: Lock around something for touching LastEdited? ++ All other references to LastEdited.
            OwningRegion.TheServer.Schedule.StartASyncTask(() =>
            {
                SaveToFileI();
                if (callback != null)
                {
                    callback.Invoke();
                }
            });
        }

        /// <summary>
        /// Asyncable (math).
        /// </summary>
        public string GetFileName()
        {
            return "saves/" + OwningRegion.Name.ToLower() + "/chunks/" + WorldPosition.Z + "/" + WorldPosition.Y + "/" + WorldPosition.X + ".chk";
        }

        void SaveToFileI()
        {
            lock (SaveLock)
            {
                try
                {
                    Program.Files.WriteBytes(GetFileName(), GetSaveData());
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Saving chunk to file: " + ex.ToString());
                }
            }
        }

        /// <summary>
        /// Asyncable.
        /// TODO: Local edit session lock.
        /// Note: Set LOADING=true before calling, and LOADING=false when done.
        /// </summary>
        public void LoadFromSaveData(byte[] data)
        {
            byte[] bytes = FileHandler.UnGZip(data);
            string engine = Encoding.ASCII.GetString(bytes, 0, 4);
            if (engine != "VOX_")
            {
                throw new Exception("Invalid save data ENGINE format: " + engine + "!");
            }
            int revision = Utilities.BytesToInt(Utilities.BytesPartial(bytes, 4, 4));
            if (revision == 1)
            {
                for (int i = 0; i < BlocksInternal.Length; i++)
                {
                    BlocksInternal[i].BlockMaterial = Utilities.BytesToUshort(Utilities.BytesPartial(bytes, 8 + i * 2, 2));
                    BlocksInternal[i].BlockData = bytes[8 + BlocksInternal.Length * 2 + i];
                    BlocksInternal[i].BlockLocalData = bytes[8 + BlocksInternal.Length * 3 + i];
                }
            }
            else
            {
                throw new Exception("Invalid save data VERSION format: " + revision + "!");
            }
        }
    }
}
