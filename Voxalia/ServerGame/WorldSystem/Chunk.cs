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

        public ChunkFlags Flags = ChunkFlags.POPULATING;
        
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
        
        public FullChunkObject FCO = null;
        
        /// <summary>
        /// Sync only.
        /// Probably.
        /// </summary>
        public void AddToWorld()
        {
            if (Flags.HasFlag(ChunkFlags.ISCUSTOM))
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
