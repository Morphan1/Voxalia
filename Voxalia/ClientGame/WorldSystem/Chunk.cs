using System;
using System.Collections.Generic;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public Region OwningRegion = null;

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
                Chunk ch = OwningRegion.GetChunk(WorldPosition + loc);
                if (ch != null)
                {
                    OwningRegion.UpdateChunk(ch);
                }
            }
        }

        public void CalculateLighting()
        {
            if (OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB)
            {
                for (int x = 0; x < CSize; x++)
                {
                    for (int y = 0; y < CSize; y++)
                    {
                        byte light = 255;
                        for (int z = CSize - 1; z >= 0; z--)
                        {
                            /*Material mat = (Material)GetBlockAt(x, y, z).BlockMaterial;
                            if (mat.IsOpaque())
                            {
                                light = 0;
                            }
                            if (mat.RendersAtAll())
                            {
                                light /= 2;
                            }*/
                            BlocksInternal[BlockIndex(x, y, z)].BlockLocalData = light;
                        }
                    }
                }
            }
        }

        public InstancedMeshShape CalculateChunkShape()
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
                        if (((Material)c.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID)
                        {
                            // TODO: Handle ALL blocks against the surface when low-LOD?
                            BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(x, y, 30));
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(x, y, -1));
                            BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(x, 30, z));
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(x, -1, z));
                            BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(30, y, z));
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : OwningRegion.GetBlockInternal(new Location(ppos) + new Location(-1, y, z));
                            bool zps = ((Material)zp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                            bool zms = ((Material)zm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                            bool xps = ((Material)xp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                            bool xms = ((Material)xm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                            bool yps = ((Material)yp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                            bool yms = ((Material)ym.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                            Vector3 pos = new Vector3(x, y, z);
                            List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            foreach (Vector3 vec in vecsi)
                            {
                                Vertices.Add(vec * PosMultiplier + ppos);
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
            InstancedMeshShape shape = new InstancedMeshShape(vecs, inds);
            return shape;
        }

        public FullChunkObject FCO = null;

        public InstancedMeshShape MeshShape = null;

        public InstancedMesh worldObject = null;

        public ASyncScheduleItem adding = null;

        public void AddToWorld(Action callback = null)
        {
#if NEW_CHUNKS
            if (worldObject != null)
            {
                OwningRegion.RemoveChunkQuiet(FCO);
            }
#endif
            FCO = new FullChunkObject(WorldPosition.ToBVector() * 30, BlocksInternal);
            FCO.CollisionRules.Group = CollisionUtil.Solid;
#if NEW_CHUNKS
            OwningRegion.AddChunk(FCO);
            if (callback != null)
            {
                callback.Invoke();
            }
#else
            if (adding != null)
            {
                ASyncScheduleItem item = OwningRegion.TheClient.Schedule.AddASyncTask(() => AddInternal(callback));
                adding = adding.ReplaceOrFollowWith(item);
            }
            else
            {
                adding = OwningRegion.TheClient.Schedule.StartASyncTask(() => AddInternal(callback));
            }
#endif
        }

        public void Destroy()
        {
            if (worldObject != null)
            {
#if NEW_CHUNKS
                OwningRegion.RemoveChunkQuiet(FCO);
#else
                OwningRegion.RemoveChunkQuiet(worldObject);
#endif
                worldObject = null;
            }
            if (_VBO != null)
            {
                _VBO.Destroy();
                _VBO = null;
            }
        }

        public bool LOADING = false;
        public bool PROCESSED = false;
        public bool PRED = false;
        public bool DENIED = false;
        
        void AddInternal(Action callback)
        {
#if !NEW_CHUNKS
            InstancedMeshShape mx = CalculateChunkShape();
            InstancedMesh tregionObject = mx == null ? null: new InstancedMesh(mx);
            OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
            {
                if (worldObject != null)
                {
                    OwningRegion.RemoveChunkQuiet(worldObject);
                }
                if (DENIED)
                {
                    return;
                }
                MeshShape = mx;
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
#endif
        }
    }
}
