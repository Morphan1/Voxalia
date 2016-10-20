//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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

        public Dictionary<Vector3i, Chunk> LoadedChunks = new Dictionary<Vector3i, Chunk>();

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
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE;
            return (Material)ch.GetBlockAt(x, y, z).BlockMaterial;
        }

        public BlockInternal GetBlockInternal(Location pos)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE;
            return ch.GetBlockAt(x, y, z);
        }

        public void SetBlockMaterial(Location pos, BlockInternal bi, bool broadcast = true, bool regen = true, bool override_protection = false)
        {
            SetBlockMaterial(pos, bi.Material, bi.BlockData, bi._BlockPaintInternal, bi.BlockLocalData, bi.Damage, broadcast, regen, override_protection);
        }

        public void SetBlockMaterial(Location pos, Material mat, byte dat = 0, byte paint = 0, byte locdat = (byte)BlockFlags.EDITED, BlockDamage damage = BlockDamage.NONE,
            bool broadcast = true, bool regen = true, bool override_protection = false)
        {
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            lock (ch.EditSessionLock)
            {
                int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE;
                int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE;
                int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE;
                if (!override_protection && ((BlockFlags)ch.GetBlockAt(x, y, z).BlockLocalData).HasFlag(BlockFlags.PROTECTED))
                {
                    return;
                }
                BlockInternal bi = new BlockInternal((ushort)mat, dat, paint, locdat) { Damage = damage };
                ch.SetBlockAt(x, y, z, bi);
                ch.LastEdited = GlobalTickTime;
                ch.ChunkDetect();
                // TODO: See if this makes any new chunks visible!
                if (broadcast)
                {
                    // TODO: Send per-person based on chunk awareness details
                    ChunkSendToAll(new BlockEditPacketOut(new Location[] { pos }, new ushort[] { bi._BlockMaterialInternal }, new byte[] { dat }, new byte[] { paint }), ch.WorldPosition);
                }
            }
        }
        
        public Location[] FellLocs = new Location[] { new Location(0, 0, 1), new Location(1, 0, 0), new Location(0, 1, 0), new Location(-1, 0, 0), new Location(0, -1, 0) };

        public void BreakNaturally(Location pos, bool regentrans = true)
        {
            pos = pos.GetBlockLocation();
            Chunk ch = LoadChunk(ChunkLocFor(pos));
            lock (ch.EditSessionLock)
            {
                int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE;
                int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE;
                int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE;
                BlockInternal bi = ch.GetBlockAt(x, y, z);
                if (((BlockFlags)bi.BlockLocalData).HasFlag(BlockFlags.PROTECTED))
                {
                    return;
                }
                Material mat = (Material)bi.BlockMaterial;
                ch.BlocksInternal[ch.BlockIndex(x, y, z)].BlockLocalData |= (byte)BlockFlags.PROTECTED;
                if (mat != (ushort)Material.AIR)
                {
                    ch.SetBlockAt(x, y, z, new BlockInternal((ushort)Material.AIR, 0, 0, (byte)BlockFlags.EDITED));
                    ch.LastEdited = GlobalTickTime;
                    SurroundRunPhysics(pos);
                    if (regentrans)
                    {
                        ChunkSendToAll(new BlockEditPacketOut(new Location[] { pos }, new ushort[] { 0 }, new byte[] { 0 }, new byte[] { 0 }), ch.WorldPosition);
                    }
                    bi.Material = mat.GetBreaksInto();
                    BlockItemEntity bie = new BlockItemEntity(this, new BlockInternal((ushort)bi._BlockMaterialInternal, bi.BlockData, bi._BlockPaintInternal, 0), pos);
                    SpawnEntity(bie);
                }
            }
        }

        public Location GetBlockLocation(Location worldPos)
        {
            return new Location(Math.Floor(worldPos.X), Math.Floor(worldPos.Y), Math.Floor(worldPos.Z));
        }

        public Vector3i ChunkLocFor(Location worldPos)
        {
            Vector3i temp;
            temp.X = (int)Math.Floor(worldPos.X / (double)Chunk.CHUNK_SIZE);
            temp.Y = (int)Math.Floor(worldPos.Y / (double)Chunk.CHUNK_SIZE);
            temp.Z = (int)Math.Floor(worldPos.Z / (double)Chunk.CHUNK_SIZE);
            return temp;
        }

        public Chunk LoadChunkNoPopulate(Vector3i cpos)
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

        public Chunk LoadChunkLOD(Vector3i cpos)
        {
            byte[] lod = ChunkManager.GetLODChunkDetails(cpos.X, cpos.Y, cpos.Z);
            if (lod != null)
            {
                return new Chunk(lod) { WorldPosition = cpos };
            }
            else
            {
                return null;
            }
        }

        public Chunk LoadChunk(Vector3i cpos)
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
                    LoadedChunks.Remove(cpos);
                    ChunkManager.ClearChunkDetails(cpos);
                    SysConsole.Output(OutputType.ERROR, "non-custom chunk was still loading when grabbed: " + chunk.WorldPosition);
                }
                else
                {
                    chunk.UnloadTimer = 0;
                    return chunk;
                }
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

        void HandleChunkBGOne(Chunk chunk, Action<Chunk> callback)
        {
            if (chunk.Flags.HasFlag(ChunkFlags.ISCUSTOM))
            {
                chunk.Flags &= ~ChunkFlags.ISCUSTOM;
                chunk.LoadSchedule = TheWorld.Schedule.StartASyncTask(() =>
                {
                    chunk.UnloadTimer = 0;
                    PopulateChunk(chunk, false, false, true);
                    chunk.UnloadTimer = 0;
                    chunk.LoadSchedule = null;
                    TheWorld.Schedule.ScheduleSyncTask(() =>
                    {
                        chunk.UnloadTimer = 0;
                        chunk.AddToWorld();
                        callback.Invoke(chunk);
                    });
                });
                return;
            }
            if (chunk.Flags.HasFlag(ChunkFlags.POPULATING))
            {
                LoadedChunks.Remove(chunk.WorldPosition);
                ChunkManager.ClearChunkDetails(chunk.WorldPosition);
                SysConsole.Output(OutputType.ERROR, "non-custom chunk was still loading when grabbed: " + chunk.WorldPosition);
            }
            chunk.LoadSchedule = TheWorld.Schedule.StartASyncTask(() =>
            {
                chunk.UnloadTimer = 0;
                PopulateChunk(chunk, true, false, true);
                chunk.UnloadTimer = 0;
                chunk.LoadSchedule = null;
                TheWorld.Schedule.ScheduleSyncTask(() =>
                {
                    chunk.UnloadTimer = 0;
                    chunk.AddToWorld();
                    callback.Invoke(chunk);
                });
            });
        }
        
        public void LoadChunk_Background(Vector3i cpos, Action<Chunk> callback = null)
        {
            Chunk chunk;
            if (LoadedChunks.TryGetValue(cpos, out chunk))
            {
                if (chunk.LoadSchedule != null)
                {
                    TheWorld.Schedule.StartASyncTask(() =>
                    {
                        while (chunk.LoadSchedule != null)
                        {
                            Thread.Sleep(1); // TODO: Handle loading a loading chunk more cleanly.
                        }
                        TheWorld.Schedule.ScheduleSyncTask(() =>
                        {
                            HandleChunkBGOne(chunk, callback);
                        });
                    });
                    return;
                }
                HandleChunkBGOne(chunk, callback);
                return;
            }
            chunk = new Chunk();
            chunk.Flags = ChunkFlags.POPULATING;
            chunk.OwningRegion = this;
            chunk.WorldPosition = cpos;
            LoadedChunks.Add(cpos, chunk);
            chunk.UnloadTimer = 0;
            chunk.LoadSchedule = TheWorld.Schedule.StartASyncTask(() =>
            {
                chunk.UnloadTimer = 0;
                PopulateChunk(chunk, true, false, true);
                chunk.LoadSchedule = null;
                TheWorld.Schedule.ScheduleSyncTask(() =>
                {
                    chunk.UnloadTimer = 0;
                    chunk.AddToWorld();
                    callback.Invoke(chunk);
                });
            });
        }


        public Chunk GetChunk(Vector3i cpos)
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
            int x = (int)Math.Floor(pos.X) - (int)ch.WorldPosition.X * Chunk.CHUNK_SIZE;
            int y = (int)Math.Floor(pos.Y) - (int)ch.WorldPosition.Y * Chunk.CHUNK_SIZE;
            int z = (int)Math.Floor(pos.Z) - (int)ch.WorldPosition.Z * Chunk.CHUNK_SIZE;
            return ch.GetBlockAt(x, y, z);
        }

        public BlockPopulator Generator = new SimpleGeneratorCore();
        public BiomeGenerator BiomeGen = new SimpleBiomeGenerator();

        public bool PopulateChunk(Chunk chunk, bool allowFile, bool fileOnly = false, bool async = false)
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
                    ChunkDetails ents;
                    lock (chunk.GetLocker())
                    {
                        ents = ChunkManager.GetChunkEntities((int)chunk.WorldPosition.X, (int)chunk.WorldPosition.Y, (int)chunk.WorldPosition.Z);
                    }
                    if (dat != null)
                    {
                        if (ents == null)
                        {
                            ents = new ChunkDetails() { X = dat.X, Y = dat.Y, Z = dat.Z, Version = dat.Version, Flags = dat.Flags, Reachables = null, Blocks = new byte[0] };
                        }
                        chunk.LoadFromSaveData(dat, ents);
                        if (!chunk.Flags.HasFlag(ChunkFlags.ISCUSTOM))
                        {
                            chunk.Flags &= ~ChunkFlags.POPULATING;
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                SysConsole.Output(OutputType.ERROR, "Loading chunk: " + chunk.WorldPosition.ToString() + ": " + ex.ToString());
                return false;
            }
            if (fileOnly)
            {
                return false;
            }
            try
            {
                Generator.Populate(TheWorld.Seed, TheWorld.Seed2, TheWorld.Seed3, TheWorld.Seed4, TheWorld.Seed5, chunk);
                chunk.LastEdited = GlobalTickTime;
                chunk.Flags &= ~(ChunkFlags.POPULATING | ChunkFlags.ISCUSTOM);
                chunk.ChunkDetect();
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                SysConsole.Output(OutputType.ERROR, "Loading chunk" + chunk.WorldPosition.ToString() + ": " + ex.ToString());
                return false;
            }
            return true;
        }

        public List<Location> GetBlocksInRadius(Location pos, double rad)
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
