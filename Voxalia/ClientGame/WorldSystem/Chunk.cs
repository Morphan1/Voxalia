using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUutilities;

namespace Voxalia.ClientGame.WorldSystem
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

        public CollisionShape CalculateChunkShape()
        {
            List<Vector3> Vertices = new List<Vector3>();
            Vector3 pos = WorldPosition.ToBVector();
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                for (int y = 0; y < CHUNK_SIZE; y++)
                {
                    for (int z = 0; z < CHUNK_SIZE; z++)
                    {
                        ushort zp = z + 1 < CHUNK_SIZE ? GetBlockAt(x, y, z + 1) : (ushort)1;
                        ushort zm = z - 1 > 0 ? GetBlockAt(x, y, z - 1) : (ushort)1;
                        ushort yp = y + 1 < CHUNK_SIZE ? GetBlockAt(x, y + 1, z) : (ushort)1;
                        ushort ym = y - 1 > 0 ? GetBlockAt(x, y - 1, z) : (ushort)1;
                        ushort xp = x + 1 < CHUNK_SIZE ? GetBlockAt(x + 1, y, z) : (ushort)1;
                        ushort xm = x - 1 > 0 ? GetBlockAt(x - 1, y, z) : (ushort)1;
                        ushort zpm = MaterialHelpers.GetMaterialHardMat(zp);
                        ushort zmm = MaterialHelpers.GetMaterialHardMat(zm);
                        ushort ypm = MaterialHelpers.GetMaterialHardMat(yp);
                        ushort ymm = MaterialHelpers.GetMaterialHardMat(ym);
                        ushort xpm = MaterialHelpers.GetMaterialHardMat(xp);
                        ushort xmm = MaterialHelpers.GetMaterialHardMat(xm);
                        if (!((Material)zp).IsSolid())
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
            int[] inds = new int[Vertices.Count];
            for (int i = 0; i < inds.Length; i++)
            {
                inds[i] = i;
            }
            StaticMeshShape sms = new StaticMeshShape(Vertices.ToArray(), inds);
            return null;
        }
    }
}
