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
                byte[] bytes = new byte[12 + BlocksInternal.Length * 5];
                Encoding.ASCII.GetBytes("VOX_").CopyTo(bytes, 0); // General Header
                Utilities.IntToBytes(3).CopyTo(bytes, 4); // Saves Version
                Utilities.IntToBytes((int)Flags).CopyTo(bytes, 8);
                for (int i = 0; i < BlocksInternal.Length; i++)
                {
                    Utilities.UshortToBytes(BlocksInternal[i].BlockMaterial).CopyTo(bytes, 12 + i * 2);
                    bytes[12 + BlocksInternal.Length * 2 + i] = BlocksInternal[i].BlockData;
                    bytes[12 + BlocksInternal.Length * 3 + i] = BlocksInternal[i].BlockLocalData;
                    bytes[12 + BlocksInternal.Length * 4 + i] = BlocksInternal[i].BlockPaint;
                }
                return FileHandler.GZip(bytes);
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
                return FileHandler.GZip(ds.ToArray());
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
            byte[] saves1 = GetChunkSaveData();
            OwningRegion.TheServer.Schedule.StartASyncTask(() =>
            {
                SaveToFileI(saves1, ents);
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
            return "saves/" + OwningRegion.Name.ToLowerInvariant() + "/chunks/" + WorldPosition.Z + "/" + WorldPosition.Y + "/" + WorldPosition.X + ".chk";
        }

        void SaveToFileI(byte[] saves1, byte[] saves2)
        {
            try
            {
                byte[] res = new byte[4 + saves1.Length + 4 + saves2.Length];
                Utilities.IntToBytes(saves1.Length).CopyTo(res, 0);
                saves1.CopyTo(res, 4);
                Utilities.IntToBytes(saves2.Length).CopyTo(res, 4 + saves1.Length);
                saves2.CopyTo(res, 4 + saves1.Length + 4);
                lock (GetLocker())
                {
                    Program.Files.WriteBytes(GetFileName(), res);
                    Thread.Sleep(50);
                }
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Saving chunk " + WorldPosition.ToString() + " to file: " + ex.ToString());
            }
        }

        List<Entity> entsToSpawn = new List<Entity>();
        
        public void LoadFromSaveData(byte[] data)
        {
            int clen = Utilities.BytesToInt(Utilities.BytesPartial(data, 0, 4));
            // Begin: blocks
            byte[] bytes = FileHandler.UnGZip(Utilities.BytesPartial(data, 4, clen));
            string engine = Encoding.ASCII.GetString(bytes, 0, 4);
            if (engine != "VOX_")
            {
                throw new Exception("Invalid save data ENGINE format: " + engine + "!");
            }
            int revision = Utilities.BytesToInt(Utilities.BytesPartial(bytes, 4, 4));
            if (revision != 3)
            {
                throw new Exception("Invalid save data REVISION: " + revision + "!");
            }
            int flags = Utilities.BytesToInt(Utilities.BytesPartial(bytes, 4 + 4, 4));
            Flags = (ChunkFlags)flags & ~(ChunkFlags.POPULATING);
            for (int i = 0; i < BlocksInternal.Length; i++)
            {
                BlocksInternal[i]._BlockMaterialInternal = Utilities.BytesToUshort(Utilities.BytesPartial(bytes, 4 + 4 + 4 + i * 2, 2));
                BlocksInternal[i].BlockData = bytes[4 + 4 + 4 + BlocksInternal.Length * 2 + i];
                BlocksInternal[i].BlockLocalData = bytes[4 + 4 + 4 + BlocksInternal.Length * 3 + i];
                BlocksInternal[i].BlockPaint = bytes[4 + 4 + 4 + BlocksInternal.Length * 4 + i];
            }
            // Begin: entities
            int elen = Utilities.BytesToInt(Utilities.BytesPartial(data, 4 + clen, 4));
            byte[] ebytes = FileHandler.UnGZip(Utilities.BytesPartial(data, 4 + clen + 4, elen));
            if (ebytes.Length > 0)
            {
                using (DataStream eds = new DataStream(ebytes))
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
