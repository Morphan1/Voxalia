using System;
using System.Collections.Generic;
using System.Text;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Chunk
    {
        public static List<Object> Lockers = new List<Object>();

        static Chunk()
        {
            Lockers = new List<Object>(21);
            for (int i = 0; i < 20; i++)
            {
                Lockers.Add(new Object());
            }
        }

        Object GetLocker()
        {
            return Lockers[Math.Abs(WorldPosition.GetHashCode()) % 20];
        }

        public const int CHUNK_SIZE = 30;

        public bool LOADING = true;

        public bool POPULATING = true;

        public bool ISCUSTOM = false;

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
        /// </summary>
        public void SetBlockAt(int x, int y, int z, BlockInternal mat)
        {
            lock (EditSessionLock)
            {
                BlocksInternal[BlockIndex(x, y, z)] = mat;
            }
        }

        public double LastEdited = -1;

        /// <summary>
        /// Asyncable (Edit session).
        /// </summary>
        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            //lock (EditSessionLock) // TODO: How do we magic this to be possible?
            {
                return BlocksInternal[BlockIndex(x, y, z)];
            }
        }

        /// <summary>
        /// Asyncable (Edit session).
        /// </summary>
        /// <returns>A chunk.</returns>
        public StaticMesh CalculateChunkShape()
        {
            try
            {
                List<Vector3> Vertices = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6); // TODO: Make this an array?
                Vector3 ppos = WorldPosition.ToBVector() * 30;
                //lock (EditSessionLock) // TODO: How do we magic this to be possible?
                {
                    for (int x = 0; x < CHUNK_SIZE; x++)
                    {
                        for (int y = 0; y < CHUNK_SIZE; y++)
                        {
                            for (int z = 0; z < CHUNK_SIZE; z++)
                            {
                                BlockInternal c = GetBlockAt(x, y, z);
                                if (((Material)c.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID)
                                {
                                    BlockInternal zp = z + 1 < CHUNK_SIZE ? BlocksInternal[BlockIndex(x, y, z + 1)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(x, y, 30));
                                    BlockInternal zm = z > 0 ? BlocksInternal[BlockIndex(x, y, z - 1)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(x, y, -1));
                                    BlockInternal yp = y + 1 < CHUNK_SIZE ? BlocksInternal[BlockIndex(x, y + 1, z)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(x, 30, z));
                                    BlockInternal ym = y > 0 ? BlocksInternal[BlockIndex(x, y - 1, z)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(x, -1, z));
                                    BlockInternal xp = x + 1 < CHUNK_SIZE ? BlocksInternal[BlockIndex(x + 1, y, z)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(30, y, z));
                                    BlockInternal xm = x > 0 ? BlocksInternal[BlockIndex(x - 1, y, z)] : OwningRegion.GetBlockInternal_NoLoad(new Location(ppos) + new Location(-1, y, z));
                                    bool zps = ((Material)zp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                                    bool zms = ((Material)zm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                                    bool xps = ((Material)xp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                                    bool xms = ((Material)xm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                                    bool yps = ((Material)yp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                                    bool yms = ((Material)ym.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                                    Vector3 pos = new Vector3(ppos.X + x, ppos.Y + y, ppos.Z + z);
                                    List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                                    Vertices.AddRange(vecsi);
                                }
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
            catch (Exception ex)
            {
                SysConsole.Output("Building chunk: " + WorldPosition, ex);
                return null;
            }
        }

        public FullChunkObject FCO = null;
        
        public ASyncScheduleItem adding = null;
        
        /// <summary>
        /// Sync only.
        /// Probably.
        /// TODO: Async? (Probably just a single local Add lock is sufficient).
        /// </summary>
        public void AddToWorld()
        {
            if (ISCUSTOM)
            {
                return;
            }
            if (FCO != null)
            {
                return;
            }
            FCO = new FullChunkObject(WorldPosition.ToBVector() * 30, BlocksInternal);
            FCO.CollisionRules.Group = CollisionUtil.Solid;
            OwningRegion.AddChunk(FCO);
        }

        public Object EditSessionLock = new Object();
        
        /// <summary>
        /// Asyncable.
        /// TODO: Local edit session lock.
        /// </summary>
        public byte[] GetSaveData()
        {
            lock (EditSessionLock)
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
        }
        
        /// <summary>
        /// Sync only.
        /// Call OwningWorld.AddChunk(null) to recalculate after done.
        /// </summary>
        public void UnloadSafely(Action callback = null)
        {
            if (FCO != null)
            {
                OwningRegion.RemoveChunkQuiet(FCO);
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
            try
            {
                byte[] saves = GetSaveData();
                lock (GetLocker())
                {
                    Program.Files.WriteBytes(GetFileName(), saves);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving chunk to file: " + ex.ToString());
            }
        }

        /// <summary>
        /// Asyncable.
        /// Note: Set LOADING=true before calling, and LOADING=false when done.
        /// </summary>
        public void LoadFromSaveData(byte[] data)
        {
            // TODO: Lock safely?
            byte[] bytes = FileHandler.UnGZip(data);
            string engine = Encoding.ASCII.GetString(bytes, 0, 4);
            if (engine != "VOX_")
            {
                throw new Exception("Invalid save data ENGINE format: " + engine + "!");
            }
            int revision = Utilities.BytesToInt(Utilities.BytesPartial(bytes, 4, 4));
            if (revision != 1)
            {
                throw new Exception("Invalid save data REVISION: " + revision + "!");
            }
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BlocksInternal[i].BlockMaterial = Utilities.BytesToUshort(Utilities.BytesPartial(bytes, 8 + i * 2, 2));
                BlocksInternal[i].BlockData = bytes[8 + BlocksInternal.Length * 2 + i];
                BlocksInternal[i].BlockLocalData = bytes[8 + BlocksInternal.Length * 3 + i];
            }
        }
    }
}
