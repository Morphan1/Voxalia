using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public World OwningWorld = null;

        public Location WorldPosition;

        public int CSize = CHUNK_SIZE;

        public int PosMultiplier;

        public Chunk(int posMult)
        {
            PosMultiplier = posMult;
            CSize = CHUNK_SIZE / posMult;
            BlocksInternal = new BlockInternal[CSize * CSize * CSize];
        }

        public BlockInternal[] BlocksInternal;
        
        public int BlockIndex(int x, int y, int z)
        {
            return z * CSize * CSize + y * CSize + x;
        }

        public void SetBlockAt(int x, int y, int z, BlockInternal mat)
        {
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }

        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }

        static Location[] slocs = new Location[] { new Location(1, 0, 0), new Location(-1, 0, 0), new Location(0, 1, 0),
            new Location(0, -1, 0), new Location(0, 0, 1), new Location(0, 0, -1) };

        public void UpdateSurroundingsFully()
        {
            foreach (Location loc in slocs)
            {
                Chunk ch = OwningWorld.GetChunk(WorldPosition + loc);
                if (ch != null)
                {
                    ch.AddToWorld();
                    ch.CreateVBO();
                }
            }
        }

        public StaticMesh CalculateChunkShape()
        {
            List<Vector3> Vertices = new List<Vector3>(CSize * CSize * CSize * 6); // TODO: Make this an array?
            Vector3 ppos = WorldPosition.ToBVector() * 30;
            for (int x = 0; x < CSize; x++)
            {
                for (int y = 0; y < CSize; y++)
                {
                    for (int z = 0; z < CSize; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        if (((Material)c.BlockMaterial).IsSolid())
                        {
                            BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(x, y, 30));
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(x, y, -1));
                            BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(x, 30, z));
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(x, -1, z));
                            BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(30, y, z));
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : OwningWorld.GetBlockInternal(Location.FromBVector(ppos) + new Location(-1, y, z));
                            bool zps = ((Material)zp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[zp.BlockData].OccupiesTOP();
                            bool zms = ((Material)zm.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[zm.BlockData].OccupiesBOTTOM();
                            bool xps = ((Material)xp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXP();
                            bool xms = ((Material)xm.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXM();
                            bool yps = ((Material)yp.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYP();
                            bool yms = ((Material)ym.BlockMaterial).IsSolid() && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYM();
                            Vector3 pos = new Vector3(ppos.X + x, ppos.Y + y, ppos.Z + z);
                            List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            foreach (Vector3 vec in vecsi)
                            {
                                Vertices.Add((vec - pos) * PosMultiplier + pos); // TODO: Remove 'pos' from GetVertices, add it here
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

        StaticMesh worldObject = null;

        public ASyncScheduleItem adding = null;

        public void AddToWorld()
        {
            if (adding != null)
            {
                ASyncScheduleItem item = OwningWorld.TheClient.Schedule.AddASyncTask(() => AddInternal());
                adding.FollowWith(item);
                adding = item;
            }
            else
            {
                adding = OwningWorld.TheClient.Schedule.StartASyncTask(() => AddInternal());
            }
        }

        public void Destroy()
        {
            if (worldObject != null)
            {
                OwningWorld.PhysicsWorld.Remove(worldObject);
            }
            if (_VBO != null)
            {
                _VBO.Destroy();
            }
        }

        void AddInternal()
        {
            StaticMesh tworldObject = CalculateChunkShape();
            OwningWorld.TheClient.Schedule.ScheduleSyncTask(() =>
            {
                if (worldObject != null)
                {
                    OwningWorld.PhysicsWorld.Remove(worldObject);
                }
                worldObject = tworldObject;
                if (worldObject != null)
                {
                    OwningWorld.PhysicsWorld.Add(worldObject);
                }
            });
        }
    }
}
