using System;
using System.Collections.Generic;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public const int CHUNK_SIZE = FullChunkShape.CHUNK_SIZE;

        public Region OwningRegion = null;

        public Vector3i WorldPosition;

        public int CSize = CHUNK_SIZE;

        public bool[] Reachability = new bool[(int)ChunkReachability.COUNT] { true, true, true, true, true, true, true, true, true, true, true, true, true, true, true };

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

        public BlockInternal GetBlockAtLOD(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x / PosMultiplier, y / PosMultiplier, z / PosMultiplier)];
        }

        static Vector3i[] slocs = new Vector3i[] { new Vector3i(1, 0, 0), new Vector3i(-1, 0, 0), new Vector3i(0, 1, 0),
            new Vector3i(0, -1, 0), new Vector3i(0, 0, 1), new Vector3i(0, 0, -1) };

        public void UpdateSurroundingsFully()
        {
            foreach (Vector3i loc in slocs)
            {
                Chunk ch = OwningRegion.GetChunk(WorldPosition + loc);
                if (ch != null)
                {
                    OwningRegion.UpdateChunk(ch);
                }
            }
        }

        /*
        public void CalculateLighting()
        {
            if (OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB)
            {
                for (int x = 0; x < CSize; x++)
                {
                    for (int y = 0; y < CSize; y++)
                    {
                        float light = 1f;
                        for (int z = CSize - 1; z >= 0; z--)
                        {
                            BlocksInternal[BlockIndex(x, y, z)].BlockLocalData = (byte)(light * 255);
                            BlockInternal bi = GetBlockAt(x, y, z);
                            if (bi.IsOpaque())
                            {
                                light = 0;
                            }
                            if (((Material)bi.BlockMaterial).RendersAtAll())
                            {
                                light /= 1.8f;
                            }
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < BlocksInternal.Length; i++)
                {
                    BlocksInternal[i].BlockLocalData = 255;
                }
            }
        }
        */
        
        public FullChunkObject FCO = null;
        
        public ASyncScheduleItem adding = null;

        public void AddToWorld()
        {
            if (FCO != null)
            {
                return;
            }
            if (CSize == CHUNK_SIZE)
            {
                FCO = new FullChunkObject(WorldPosition.ToVector3() * CHUNK_SIZE, BlocksInternal);
                FCO.CollisionRules.Group = CollisionUtil.WorldSolid;
                OwningRegion.AddChunk(FCO);
            }
        }

        /// <summary>
        /// Sync only.
        /// </summary>
        public void Destroy()
        {
            if (FCO != null)
            {
                OwningRegion.RemoveChunkQuiet(FCO);
            }
            if (_VBO != null)
            {
                VBO tV = _VBO;
                lock (OwningRegion.TheClient.vbos)
                {
                    if (OwningRegion.TheClient.vbos.Count < 120)
                    {
                        OwningRegion.TheClient.vbos.Push(tV);
                    }
                    else
                    {
                        tV.Destroy();
                    }
                }
                _VBO = null;
            }
            DestroyPlants();
        }

        public void DestroyPlants()
        {
            if (Plant_VAO != -1)
            {
                GL.DeleteVertexArray(Plant_VAO);
                GL.DeleteBuffer(Plant_VBO_Pos);
                GL.DeleteBuffer(Plant_VBO_Ind);
                Plant_VAO = -1;
            }
        }

        public bool LOADING = false;
        public bool PROCESSED = false;
        public bool PRED = false;
        public bool DENIED = false;
    }
}
