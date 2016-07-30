using System;
using System.Collections.Generic;
using System.Text;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Files;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.EntitySystem;
using System.Threading;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Chunk
    {
        public const int CHUNK_SIZE = FullChunkShape.CHUNK_SIZE;

        public const int RAM_USAGE = CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 5;

        public static List<Object> Lockers = new List<Object>();

        static Chunk()
        {
            Lockers = new List<Object>(21);
            for (int i = 0; i < 20; i++)
            {
                Lockers.Add(new Object());
            }
        }

        public Object GetLocker()
        {
            return Lockers[Math.Abs(WorldPosition.GetHashCode()) % 20];
        }

        public byte[] LOD = null;

        public bool LOD_Is_Air = false;

        public ChunkFlags Flags = ChunkFlags.NONE;
        
        public Region OwningRegion = null;

        public Vector3i WorldPosition;

        public volatile ASyncScheduleItem LoadSchedule = null;

        public Chunk()
        {
            BlocksInternal = new BlockInternal[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        }

        public Chunk(byte[] _lod)
        {
            LOD = _lod;
            LOD_Is_Air = true;
            for (int i = 0; i < _lod.Length; i++)
            {
                if (_lod[i] != 0)
                {
                    LOD_Is_Air = false;
                    break;
                }
            }
        }

        public bool Contains(Location loc)
        {
            return loc.X >= WorldPosition.X * CHUNK_SIZE && loc.Y >= WorldPosition.Y * CHUNK_SIZE && loc.Z >= WorldPosition.Z * CHUNK_SIZE
                && loc.X < WorldPosition.X * CHUNK_SIZE + CHUNK_SIZE && loc.Y < WorldPosition.Y * CHUNK_SIZE + CHUNK_SIZE && loc.Z < WorldPosition.Z * CHUNK_SIZE + CHUNK_SIZE;
        }

        public BlockInternal[] BlocksInternal;

        public Material LODBlock(int x, int y, int z, int lod)
        {
            int xs = x * lod;
            int ys = y * lod;
            int zs = z * lod;
            Material mat = Material.AIR;
            for (int tz = lod - 1; tz >= 0; tz--)
            {
                Material c = GetBlockAt(xs, ys, zs + tz).Material;
                if (c.IsOpaque())
                {
                    return c;
                }
                else if (c != Material.AIR)
                {
                    mat = c;
                }
            }
            return mat;
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
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }

        public double LastEdited = -1;

        /// <summary>
        /// Asyncable (Edit session).
        /// </summary>
        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }
        
        public FullChunkObject FCO = null;

        public void ChunkDetect()
        {
            chunkAccessDetection = chunkAccessDetection == null ? OwningRegion.TheServer.Schedule.StartASyncTask(DetectChunkAccess) : chunkAccessDetection.ReplaceOrFollowWith(OwningRegion.TheServer.Schedule.AddASyncTask(DetectChunkAccess));
        }
        
        /// <summary>
        /// Sync only.
        /// </summary>
        public void AddToWorld()
        {
            foreach (Entity e in entsToSpawn)
            {
                OwningRegion.SpawnEntity(e);
            }
            entsToSpawn.Clear();
            foreach (SyncScheduleItem s in fixesToRun)
            {
                s.MyAction.Invoke();
            }
            fixesToRun.Clear();
            if (Flags.HasFlag(ChunkFlags.ISCUSTOM))
            {
                return;
            }
            if (FCO != null)
            {
                return;
            }
            FCO = new FullChunkObject(WorldPosition.ToVector3() * CHUNK_SIZE, BlocksInternal);
            FCO.CollisionRules.Group = CollisionUtil.WorldSolid;
            OwningRegion.AddChunk(FCO);
            OwningRegion.AddCloudsToNewChunk(this);
        }

        public ASyncScheduleItem chunkAccessDetection = null;

        public Object EditSessionLock = new Object();
        
        public byte[] GetChunkSaveData()
        {
            lock (EditSessionLock)
            {
                byte[] bytes = new byte[BlocksInternal.Length * 5];
                for (int i = 0; i < BlocksInternal.Length; i++)
                {
                    Utilities.UshortToBytes(BlocksInternal[i]._BlockMaterialInternal).CopyTo(bytes, i * 2);
                    bytes[BlocksInternal.Length * 2 + i] = BlocksInternal[i].BlockData;
                    bytes[BlocksInternal.Length * 3 + i] = BlocksInternal[i].BlockLocalData;
                    bytes[BlocksInternal.Length * 4 + i] = BlocksInternal[i]._BlockPaintInternal;
                }
                return bytes;
            }
        }

        public byte[] GetEntitySaveData()
        {
            using (DataStream ds = new DataStream())
            {
                DataWriter dw = new DataWriter(ds);
                for (int i = 0; i < OwningRegion.Entities.Count; i++)
                {
                    if (OwningRegion.Entities[i].CanSave && Contains(OwningRegion.Entities[i].GetPosition()))
                    {
                        byte[] dat = OwningRegion.Entities[i].GetSaveBytes();
                        if (dat != null)
                        {
                            dw.WriteInt((int)OwningRegion.Entities[i].GetEntityType());
                            dw.WriteFullBytes(dat);
                        }
                    }
                }
                dw.Flush();
                return ds.ToArray();
            }
        }

        void clearentities()
        {
            // TODO: Efficiency
            for (int i = 0; i < OwningRegion.Entities.Count; i++)
            {
                if (Contains(OwningRegion.Entities[i].GetPosition()))
                {
                    OwningRegion.Entities[i].RemoveMe();
                }
            }
        }

        /// <summary>
        /// Sync only.
        /// </summary>
        public void UnloadSafely(Action callback = null)
        {
            SaveAsNeeded(callback);
            clearentities();
            if (FCO != null)
            {
                OwningRegion.RemoveChunkQuiet(FCO);
                FCO = null;
            }
            OwningRegion.RemoveCloudsFrom(this);
        }

        public double UnloadTimer = 0;

        /// <summary>
        /// Sync only.
        /// </summary>
        public void SaveAsNeeded(Action callback = null)
        {
            if (FCO == null)
            {
                if (callback != null)
                {
                    callback.Invoke();
                }
                return;
            }
            if (LastEdited == -1)
            {
                byte[] ents = GetEntitySaveData();
                OwningRegion.TheServer.Schedule.StartASyncTask(() =>
                {
                    SaveToFileE(ents);
                    if (callback != null)
                    {
                        callback.Invoke();
                    }
                });
            }
            else
            {
                SaveToFile(callback);
            }
        }
        
        /// <summary>
        /// Sync only.
        /// </summary>
        public void SaveToFile(Action callback = null)
        {
            if (FCO == null)
            {
                if (callback != null)
                {
                    callback.Invoke();
                }
                return;
            }
            LastEdited = -1;
            byte[] ents = GetEntitySaveData();
            byte[] blks = GetChunkSaveData();
            OwningRegion.TheServer.Schedule.StartASyncTask(() =>
            {
                SaveToFileI(blks);
                SaveToFileE(ents);
                if (callback != null)
                {
                    callback.Invoke();
                }
            });
        }

        public bool[] Reachability = new bool[(int)ChunkReachability.COUNT] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };

        public void DetectChunkAccess()
        {
            for (int i = 0; i < (int)ChunkReachability.COUNT; i++)
            {
                // TODO: REUSE DATA WHERE POSSIBLE? Should only trace once from each direction!
                // TODO: REIMPLEMENT BUT SPEEDIER!
                Reachability[i] = FCO.ChunkShape.CanReach(FullChunkShape.ReachStarts[i], FullChunkShape.ReachEnds[i]);
            }
        }

        public byte[] LODBytes(int lod, bool canReturnNull = false)
        {
            if (LOD != null && lod == 5)
            {
                if (canReturnNull && LOD_Is_Air)
                {
                    return null;
                }
                return LOD;
            }
            bool isAir = canReturnNull;
            int csize = Chunk.CHUNK_SIZE / lod;
            byte[] data_orig = new byte[csize * csize * csize * 2];
            for (int x = 0; x < csize; x++)
            {
                for (int y = 0; y < csize; y++)
                {
                    for (int z = 0; z < csize; z++)
                    {
                        ushort mat = (ushort)LODBlock(x, y, z, lod);
                        if (mat != 0)
                        {
                            isAir = false;
                        }
                        int sp = (z * csize * csize + y * csize + x) * 2;
                        data_orig[sp] = (byte)(mat & 0xFF);
                        data_orig[sp + 1] = (byte)((mat >> 8) & 0xFF);
                    }
                }
            }
            if (isAir)
            {
                return null;
            }
            return data_orig;
        }
        
        void SaveToFileI(byte[] blks)
        {
            try
            {
                ChunkDetails det = new ChunkDetails();
                det.Version = 2;
                det.X = (int)WorldPosition.X;
                det.Y = (int)WorldPosition.Y;
                det.Z = (int)WorldPosition.Z;
                det.Flags = Flags;
                det.Blocks = blks;
                det.Reachables = new byte[(int)ChunkReachability.COUNT];
                for (int i = 0; i < det.Reachables.Length; i++)
                {
                    det.Reachables[i] = (byte)(Reachability[i] ? 1 : 0);
                }
                byte[] lod = LODBytes(5);
                lock (GetLocker())
                {
                    OwningRegion.ChunkManager.WriteChunkDetails(det);
                    OwningRegion.ChunkManager.WriteLODChunkDetails(det.X, det.Y, det.Z, lod);
                }
                OwningRegion.TheServer.BlockImages.RenderChunk(OwningRegion, WorldPosition, this);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving chunk " + WorldPosition.ToString() + " to file: " + ex.ToString());
            }
        }

        void SaveToFileE(byte[] ents)
        {
            try
            {
                ChunkDetails det = new ChunkDetails();
                det.Version = 2;
                det.X = (int)WorldPosition.X;
                det.Y = (int)WorldPosition.Y;
                det.Z = (int)WorldPosition.Z;
                det.Blocks = ents;
                lock (GetLocker())
                {
                    OwningRegion.ChunkManager.WriteChunkEntities(det);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving entities for chunk " + WorldPosition.ToString() + " to file: " + ex.ToString());
            }
        }

        public List<Entity> entsToSpawn = new List<Entity>();

        public List<SyncScheduleItem> fixesToRun = new List<SyncScheduleItem>();
        
        public void LoadFromSaveData(ChunkDetails det, ChunkDetails ents)
        {
            if (det.Version != 2 || ents.Version != 2)
            {
                throw new Exception("invalid save data VERSION: " + det.Version + " and " + ents.Version + "!");
            }
            Flags = det.Flags & ~(ChunkFlags.POPULATING);
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BlocksInternal[i]._BlockMaterialInternal = Utilities.BytesToUshort(Utilities.BytesPartial(det.Blocks, i * 2, 2));
                BlocksInternal[i].BlockData = det.Blocks[BlocksInternal.Length * 2 + i];
                BlocksInternal[i].BlockLocalData = det.Blocks[BlocksInternal.Length * 3 + i];
                BlocksInternal[i]._BlockPaintInternal = det.Blocks[BlocksInternal.Length * 4 + i];
            }
            for (int i = 0; i < Reachability.Length; i++)
            {
                Reachability[i] = det.Reachables[i] == 1;
            }
            if (ents.Blocks.Length > 0)
            {
                using (DataStream eds = new DataStream(ents.Blocks))
                {
                    DataReader edr = new DataReader(eds);
                    while (edr.BaseStream.Length - eds.Position > 7)
                    {
                        int EType = edr.ReadInt();
                        byte[] dat = edr.ReadFullBytes();
                        try
                        {
                            entsToSpawn.Add(OwningRegion.ConstructorFor((EntityType)EType).Create(OwningRegion, dat));
                        }
                        catch (Exception ex)
                        {
                            Utilities.CheckException(ex);
                            SysConsole.Output("Spawning an entity of type " + EType, ex);
                        }
                    }
                }
            }
        }
    }
}
