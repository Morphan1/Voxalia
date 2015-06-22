using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using BEPUphysics.Entities;
using BEPUphysics.BroadPhaseEntries;

namespace Voxalia.ServerGame.WorldSystem
{
    public class Chunk
    {
        public const int CHUNK_SIZE = 30;

        public World OwningWorld = null;

        public Location WorldPosition;

        public ushort[] BlocksInternal = new ushort[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
        
        public int BlockIndex(int x, int y, int z)
        {
            return z * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + x;
        }

        public void SetBlockAt(int x, int y, int z, ushort mat)
        {
            BlocksInternal[BlockIndex(x, y, z)] = mat;
        }

        public ushort GetBlockAt(int x, int y, int z)
        {
            return BlocksInternal[BlockIndex(x, y, z)];
        }

        public StaticMesh CalculateChunkShape()
        {
            List<Vector3> Vertices = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6); // TODO: Make me an array?
            Vector3 pos = WorldPosition.ToBVector();
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        ushort c = GetBlockAt(x, y, z);
                        ushort zp = z + 1 < CHUNK_SIZE ? GetBlockAt(x, y, z + 1) : (ushort)0;
                        ushort zm = z - 1 > 0 ? GetBlockAt(x, y, z - 1) : (ushort)0;
                        ushort yp = y + 1 < CHUNK_SIZE ? GetBlockAt(x, y + 1, z) : (ushort)0;
                        ushort ym = y - 1 > 0 ? GetBlockAt(x, y - 1, z) : (ushort)0;
                        ushort xp = x + 1 < CHUNK_SIZE ? GetBlockAt(x + 1, y, z) : (ushort)0;
                        ushort xm = x - 1 > 0 ? GetBlockAt(x - 1, y, z) : (ushort)0;
                        ushort cm = MaterialHelpers.GetMaterialHardMat(c);
                        ushort zpm = MaterialHelpers.GetMaterialHardMat(zp);
                        ushort zmm = MaterialHelpers.GetMaterialHardMat(zm);
                        ushort ypm = MaterialHelpers.GetMaterialHardMat(yp);
                        ushort ymm = MaterialHelpers.GetMaterialHardMat(ym);
                        ushort xpm = MaterialHelpers.GetMaterialHardMat(xp);
                        ushort xmm = MaterialHelpers.GetMaterialHardMat(xm);
                        if (((Material)cm).IsOpaque() || ((Material)cm).IsSolid()) // TODO: Better check. OccupiesFullBlock()?
                        {
                            if (!((Material)zpm).IsOpaque())
                            {
                                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                            }
                            // TODO: zm, yp, ym, xp, xm
                            // TODO: Else, handle special case direction data
                        }
                    }
                }
            }
            if (Vertices.Count == 0)
            {
                return null;
            }
            int[] inds = new int[Vertices.Count];
            for (int i = 0; i < inds.Length; i++)
            {
                inds[i] = i;
            }
            Vector3[] vecs = Vertices.ToArray();
            StaticMesh sm = new StaticMesh(vecs, inds);
            return sm;
        }

        StaticMesh worldObject = null;

        public void AddToWorld()
        {
            if (worldObject != null)
            {
                OwningWorld.TheServer.PhysicsWorld.Remove(worldObject);
            }
            worldObject = CalculateChunkShape();
            if (worldObject != null)
            {
                OwningWorld.TheServer.PhysicsWorld.Add(worldObject);
            }
        }
    }
}
