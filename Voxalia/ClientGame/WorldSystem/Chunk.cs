//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.BroadPhaseEntries;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics.OpenGL4;
using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int BlockIndex(int x, int y, int z)
        {
            return z * (CSize * CSize) + y * CSize + x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBlockAt(int x, int y, int z, BlockInternal mat)
        {
            if (SucceededBy != null)
            {
                SucceededBy.SetBlockAt(x, y, z, mat);
            }
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BlockInternal GetBlockAtLOD(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x / PosMultiplier, y / PosMultiplier, z / PosMultiplier)];
        }
        
        public FullChunkObject FCO = null;
        
        public ASyncScheduleItem adding = null;

        public void AddToWorld()
        {
            if (FCO == null && CSize == CHUNK_SIZE)
            {
                FCO = new FullChunkObject(WorldPosition.ToVector3() * CHUNK_SIZE, BlocksInternal);
                FCO.CollisionRules.Group = CollisionUtil.WorldSolid;
                OwningRegion.AddChunk(FCO);
                IsAdded = true;
            }
        }

        bool IsAdded = false;

        /// <summary>
        /// Sync only.
        /// </summary>
        public void Destroy()
        {
            if (FCO != null && IsAdded)
            {
                OwningRegion.RemoveChunkQuiet(FCO);
                IsAdded = false;
            }
            if (_VBO != null)
            {
                VBO tV = _VBO;
                lock (OwningRegion.TheClient.vbos)
                {
                    if (tV.generated && OwningRegion.TheClient.vbos.Count < 120)
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
                GL.DeleteBuffer(Plant_VBO_Col);
                GL.DeleteBuffer(Plant_VBO_Ind);
                GL.DeleteBuffer(Plant_VBO_Tcs);
                Plant_VAO = -1;
            }
        }

        public bool LOADING = false;
        public bool PROCESSED = false;
        public bool PRED = false;
        public bool DENIED = false;
    }
}
