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
        public const int CHUNK_SIZE = 30;

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

        public ChunkFlags Flags = ChunkFlags.NONE;
        
        public Region OwningRegion = null;

        public Location WorldPosition;

        public volatile ASyncScheduleItem LoadSchedule = null;

        public Chunk()
        {
            BlocksInternal = new BlockInternal[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
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
            for (int tz = lod - 1; tz >= 0; tz--)
            {
                Material c = GetBlockAt(xs, ys, zs + tz).Material;
                if (c != Material.AIR)
                {
                    return c;
                }
            }
            return Material.AIR;
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
            if (Flags.HasFlag(ChunkFlags.ISCUSTOM))
            {
                return;
            }
            if (FCO != null)
            {
                return;
            }
            FCO = new FullChunkObject(WorldPosition.ToBVector() * 30, BlocksInternal);
            FCO.CollisionRules.Group = CollisionUtil.WorldSolid;
            OwningRegion.AddChunk(FCO);
            OwningRegion.AddCloudsToNewChunk(this);
        }

        public Object EditSessionLock = new Object();
        
        public byte[] GetChunkSaveData()
        {
            lock (EditSessionLock)
            {
                byte[] bytes = new byte[BlocksInternal.Length * 5];
                for (int i = 0; i < BlocksInternal.Length; i++)
                {
                    Utilities.UshortToBytes(BlocksInternal[i].BlockMaterial).CopyTo(bytes, i * 2);
                    bytes[BlocksInternal.Length * 2 + i] = BlocksInternal[i].BlockData;
                    bytes[BlocksInternal.Length * 3 + i] = BlocksInternal[i].BlockLocalData;
                    bytes[BlocksInternal.Length * 4 + i] = BlocksInternal[i].BlockPaint;
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
            SaveToFile(callback);
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
                SaveToFileI(blks, ents);
                if (callback != null)
                {
                    callback.Invoke();
                }
            });
        }
        
        void SaveToFileI(byte[] blks, byte[] ents)
        {
            try
            {
                ChunkDetails det = new ChunkDetails();
                det.Version = 1;
                det.X = (int)WorldPosition.X;
                det.Y = (int)WorldPosition.Y;
                det.Z = (int)WorldPosition.Z;
                det.Flags = Flags;
                det.Blocks = blks;
                det.Entities = ents;
                lock (GetLocker())
                {
                    OwningRegion.ChunkManager.WriteChunkDetails(det);
                }
                OwningRegion.TheServer.BlockImages.RenderChunk(OwningRegion, WorldPosition, this);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving chunk " + WorldPosition.ToString() + " to file: " + ex.ToString());
            }
        }

        List<Entity> entsToSpawn = new List<Entity>();
        
        public void LoadFromSaveData(ChunkDetails det)
        {
            if (det.Version != 1)
            {
                throw new Exception("invalid save data VERSION: " + det.Version + "!");
            }
            Flags = det.Flags & ~(ChunkFlags.POPULATING);
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BlocksInternal[i]._BlockMaterialInternal = Utilities.BytesToUshort(Utilities.BytesPartial(det.Blocks, i * 2, 2));
                BlocksInternal[i].BlockData = det.Blocks[BlocksInternal.Length * 2 + i];
                BlocksInternal[i].BlockLocalData = det.Blocks[BlocksInternal.Length * 3 + i];
                BlocksInternal[i].BlockPaint = det.Blocks[BlocksInternal.Length * 4 + i];
            }
            if (det.Entities.Length > 0)
            {
                using (DataStream eds = new DataStream(det.Entities))
                {
                    DataReader edr = new DataReader(eds);
                    while (eds.Length - eds.Position > 0)
                    {
                        int EType = edr.ReadInt();
                        byte[] dat = edr.ReadFullBytes();
                        try
                        {
                            entsToSpawn.Add(OwningRegion.ConstructorFor((EntityType)EType).Create(OwningRegion, dat));
                        }
                        catch (Exception ex)
                        {
                            SysConsole.Output("Spawning an entity of type " + EType, ex);
                        }
                    }
                }
            }
        }
    }
}
