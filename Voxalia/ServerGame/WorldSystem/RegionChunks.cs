using System;
using System.Collections.Generic;
using Voxalia.Shared;
using Voxalia.ServerGame.ServerMainSystem;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.Settings;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.JointSystem;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using BEPUutilities.Threading;
using Voxalia.ServerGame.WorldSystem.SimpleGenerator;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared.Collision;
using Voxalia.ServerGame.ItemSystem;
using Voxalia.ServerGame.ItemSystem.CommonItems;

namespace Voxalia.ServerGame.WorldSystem
{
    public partial class Region
    {
        public void AddChunk(FullChunkObject mesh)
        {
            if (mesh == null)
            {
                return;
            }
            PhysicsWorld.Add(mesh);
        }

        public void RemoveChunkQuiet(FullChunkObject mesh)
        {
            if (mesh == null)
            {
                return;
            }
            if (TheServer.ShuttingDown)
            {
                return;
            }
            PhysicsWorld.Remove(mesh);
        }

        public Dictionary<Location, Chunk> LoadedChunks = new Dictionary<Location, Chunk>();

        public bool IsAllowedToBreak(CharacterEntity ent, Location block, Material mat)
        {
            if (block.Z > TheServer.CVars.g_maxheight.ValueI || block.Z < TheServer.CVars.g_minheight.ValueI)
            {
                return false;
            }
            return mat != Material.AIR;
        }

        public bool IsAllowedToPlaceIn(CharacterEntity ent, Location block, Material mat)
        {
            if (block.Z > TheServer.CVars.g_maxheight.ValueI || block.Z < TheServer.CVars.g_minheight.ValueI)
            {
                return false;
            }
            return mat == Material.AIR;
        }

        public Material GetBlockMaterial(Location pos)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30;
            return (Material)ch.GetBlockAt(x, y, z).BlockMaterial;
        }

        public BlockInternal GetBlockInternal(Location pos)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30;
            return ch.GetBlockAt(x, y, z);
        }

        public void SetBlockMaterial(Location pos, BlockInternal bi, bool broadcast = true, bool regen = true, bool override_protection = false)
        {
            SetBlockMaterial(pos, bi.Material, bi.BlockData, bi.BlockPaint, bi.BlockLocalData, bi.Damage, broadcast, regen, override_protection);
        }

        public void SetBlockMaterial(Location pos, Material mat, byte dat = 0, byte paint = 0, byte locdat = (byte)BlockFlags.EDITED, BlockDamage damage = BlockDamage.NONE,
            bool broadcast = true, bool regen = true, bool override_protection = false)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            lock (ch.EditSessionLock)
            {
                int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30;
                int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30;
                int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30;
                if (!override_protection && ((BlockFlags)ch.GetBlockAt(x, y, z).BlockLocalData).HasFlag(BlockFlags.PROTECTED))
                {
                    return;
                }
                BlockInternal bi = new BlockInternal((ushort)mat, dat, paint, locdat) { Damage = damage };
                ch.SetBlockAt(x, y, z, bi);
                ch.LastEdited = GlobalTickTime;
                if (broadcast)
                {
                    // TODO: Send per-person based on chunk awareness details
                    ChunkSendToAll(new BlockEditPacketOut(new Location[] { pos }, new ushort[] { bi._BlockMaterialInternal }, new byte[] { dat }, new byte[] { paint }), ch.WorldPosition);
                }
            }
        }

        public Location[] FellLocs = new Location[] { new Location(0, 0, 1), new Location(1, 0, 0), new Location(0, 1, 0), new Location(-1, 0, 0), new Location(0, -1, 0) };

        public void BreakNaturally(Location pos, bool regentrans = true, int max_subbreaks = 5)
        {
            pos = pos.GetBlockLocation();
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            lock (ch.EditSessionLock)
            {
                int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30;
                int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30;
                int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30;
                BlockInternal bi = ch.GetBlockAt(x, y, z);
                if (((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.PROTECTED))
                {
                    return;
                }
                Material mat = (Material)bi.BlockMaterial;
                ch.BlocksInternal[ch.BlockIndex(x, y, z)].BlockLocalData |= (byte)BlockFlags.PROTECTED;
                if (mat != (ushort)Material.AIR)
                {
                    if (max_subbreaks > 0
                        && !((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.EDITED)
                        && (mat == Material.LOG_OAK || mat == Material.LEAVES1))
                    {
                        foreach (Location loc in FellLocs)
                        {
                            Material m2 = GetBlockMaterial(pos + loc);
                            if (m2 == Material.LOG_OAK || m2 == Material.LEAVES1)
                            {
                                BreakNaturally(pos + loc, regentrans, max_subbreaks - 1);
                            }
                        }
                    }
                    ch.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.AIR, 0, 0, (byte)BlockFlags.EDITED));
                    ch.LastEdited = GlobalTickTime;
                    SurroundRunPhysics(pos);
                    if (regentrans)
                    {
                        ChunkSendToAll(new BlockEditPacketOut(new Location[] { pos }, new ushort[] { 0 }, new byte[] { 0 }, new byte[] { 0 }), ch.WorldPosition);
                    }
                    BlockItemEntity bie = new BlockItemEntity(this, new BlockInternal((ushort)mat, bi.BlockData, bi.BlockPaint, 0), pos);
                    SpawnEntity(bie);
                }
            }
        }

        public Location GetBlockLocation(Location worldPos)
        {
            return new Location(Math.Floor(worldPos.X), Math.Floor(worldPos.Y), Math.Floor(worldPos.Z));
        }

        public Location ChunkLocFor(Location worldPos)
        {
            worldPos.X = Math.Floor(worldPos.X / 30.0);
            worldPos.Y = Math.Floor(worldPos.Y / 30.0);
            worldPos.Z = Math.Floor(worldPos.Z / 30.0);
            return worldPos;
        }

        static Location[] slocs = new Location[] { new Location(1, 0, 0), new Location(-1, 0, 0), new Location(0, 1, 0),
            new Location(0, -1, 0), new Location(0, 0, 1), new Location(0, 0, -1) };

        public Chunk LoadChunkNoPopulate(Location cpos)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(cpos, out chunk))
            {
                // Be warned, it may still be loading here!
                return chunk;
            }
            chunk = new Chunk();
            chunk.Flags = ChunkFlags.ISCUSTOM | ChunkFlags.POPULATING;
            chunk.OwningRegion = this;
            chunk.WorldPosition = cpos;
            if (PopulateChunk(chunk, true, true))
            {
                LoadedChunks.Add(cpos, chunk);
                chunk.Flags &= ~ChunkFlags.ISCUSTOM;
                chunk.AddToWorld();
            }
            chunk.LastEdited = GlobalTickTime;
            return chunk;
        }

        public Chunk LoadChunkLOD(Location cpos)
        {
            byte[] lod = ChunkManager.GetLODChunkDetails((int)cpos.X, (int)cpos.Y, (int)cpos.Z);
            if (lod != null)
            {
                return new Chunk(lod) { WorldPosition = cpos };
            }
            else
            {
                return null;
            }
        }

        public Chunk LoadChunk(Location cpos)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(cpos, out chunk))
            {
                while (chunk.LoadSchedule != null)
                {
                    Thread.Sleep(1); // TODO: Handle loading a loading chunk more cleanly.
                }
                if (chunk.Flags.HasFlag(ChunkFlags.ISCUSTOM))
                {
                    chunk.Flags &= ~ChunkFlags.ISCUSTOM;
                    PopulateChunk(chunk, false);
                    chunk.AddToWorld();
                }
                if (chunk.Flags.HasFlag(ChunkFlags.POPULATING))
                {
                    throw new Exception("Non-custom chunk was still loading when grabbed?!");
                }
                chunk.UnloadTimer = 0;
                return chunk;
            }
            chunk = new Chunk();
            chunk.Flags = ChunkFlags.POPULATING;
            chunk.OwningRegion = this;
            chunk.WorldPosition = cpos;
            LoadedChunks.Add(cpos, chunk);
            PopulateChunk(chunk, true);
            chunk.AddToWorld();
            return chunk;
        }

        void Loadchunkbgint(Chunk ch, Action<bool> callback, Scheduler schedule)
        {
            if (!ch.Flags.HasFlag(ChunkFlags.ISCUSTOM))
            {
                if (callback != null)
                {
                    callback.Invoke(true);
                }
            }
            else
            {
                ch.AddToWorld();
                ch.LoadSchedule = schedule.AddASyncTask(() =>
                {
                    PopulateChunk(ch, false);
                    ch.LoadSchedule = null;
                    schedule.ScheduleSyncTask(() =>
                    {
                        if (callback != null)
                        {
                            callback.Invoke(false);
                        }
                    });
                });
                ch.LoadSchedule.RunMe();
            }
        }

        /// <summary>
        /// Designed for startup time.
        /// </summary>
        void LoadChunk_Background_Startup(Location cpos, Action<bool> callback, Scheduler schedule)
        {
            Chunk ch;
            if (LoadedChunks.TryGetValue(cpos, out ch))
            {
                if (ch.LoadSchedule == null)
                {
                    Loadchunkbgint(ch, callback, schedule);
                }
                else
                {
                    ch.LoadSchedule.ReplaceOrFollowWith(schedule.AddASyncTask(() =>
                    {
                        schedule.ScheduleSyncTask(() =>
                        {
                            Loadchunkbgint(ch, callback, schedule);
                        });
                    }));
                }
                return;
            }
            ch = new Chunk();
            ch.OwningRegion = this;
            ch.WorldPosition = cpos;
            LoadedChunks.Add(cpos, ch);
            ch.AddToWorld();
            ch.LoadSchedule = schedule.AddASyncTask(() =>
            {
                PopulateChunk(ch, true);
                ch.LoadSchedule = null;
                schedule.ScheduleSyncTask(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(false);
                    }
                });
            });
            ch.LoadSchedule.RunMe();
        }

        public void LoadChunk_Background(Location cpos, Action<bool> callback = null)
        {
            TheServer.Schedule.ScheduleSyncTask(() =>
            {
                LoadChunk_Background_Startup(cpos, callback, TheServer.Schedule);
            });
        }


        public Chunk GetChunk(Location cpos)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(cpos, out chunk))
            {
                if (chunk.Flags.HasFlag(ChunkFlags.ISCUSTOM))
                {
                    return null;
                }
                return chunk;
            }
            return null;
        }

        public BlockInternal GetBlockInternal_NoLoad(Location pos)
        {
            Chunk ch = GetChunk(ChunkLocFor(pos));
            if (ch == null)
            {
                return BlockInternal.AIR;
            }
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * 30;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * 30;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * 30;
            return ch.GetBlockAt(x, y, z);
        }

        public BlockPopulator Generator = new SimpleGeneratorCore();
        public BiomeGenerator BiomeGen = new SimpleBiomeGenerator();

        public bool PopulateChunk(Chunk chunk, bool allowFile, bool fileOnly = false)
        {
            try
            {
                if (allowFile)
                {
                    ChunkDetails dat;
                    lock (chunk.GetLocker())
                    {
                        dat = ChunkManager.GetChunkDetails((int)chunk.WorldPosition.X, (int)chunk.WorldPosition.Y, (int)chunk.WorldPosition.Z);
                    }
                    if (dat != null)
                    {
                        chunk.LoadFromSaveData(dat);
                        TheServer.Schedule.ScheduleSyncTask(() =>
                        {
                            chunk.AddToWorld();
                        });
                        if (!chunk.Flags.HasFlag(ChunkFlags.ISCUSTOM))
                        {
                            chunk.Flags &= ~ChunkFlags.POPULATING;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                SysConsole.Output(OutputType.ERROR, "Loading chunk" + chunk.WorldPosition.ToString() + ": " + ex.ToString());
                return false;
            }
            if (fileOnly)
            {
                return false;
            }
            Generator.Populate(Seed, Seed2, Seed3, Seed4, Seed5, chunk);
            chunk.LastEdited = GlobalTickTime;
            chunk.Flags &= ~(ChunkFlags.POPULATING | ChunkFlags.ISCUSTOM);
            return true;
        }

        public List<Location> GetBlocksInRadius(Location pos, float rad)
        {
            int min = (int)Math.Floor(-rad);
            int max = (int)Math.Ceiling(rad);
            List<Location> posset = new List<Location>();
            for (int x = min; x < max; x++)
            {
                for (int y = min; y < max; y++)
                {
                    for (int z = min; z < max; z++)
                    {
                        Location post = new Location(pos.X + x, pos.Y + y, pos.Z + z);
                        if ((post - pos).LengthSquared() <= rad * rad)
                        {
                            posset.Add(post);
                        }
                    }
                }
            }
            return posset;
        }

        public bool InWater(Location min, Location max)
        {
            // TODO: Efficiency!
            min = min.GetBlockLocation();
            max = max.GetUpperBlockBorder();
            for (int x = (int)min.X; x < max.X; x++)
            {
                for (int y = (int)min.Y; y < max.Y; y++)
                {
                    for (int z = (int)min.Z; z < max.Z; z++)
                    {
                        if (((Material)GetBlockInternal_NoLoad(min + new Location(x, y, z)).BlockMaterial).GetSolidity() == MaterialSolidity.LIQUID)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
