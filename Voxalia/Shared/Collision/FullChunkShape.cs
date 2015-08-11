using BEPUphysics.CollisionShapes;
using BEPUutilities;
using System;

namespace Voxalia.Shared.Collision
{
    public class FullChunkShape : CollisionShape
    {
        public const int CHUNK_SIZE = 30;

        public FullChunkShape(BlockInternal[] blocks)
        {
            Blocks = blocks;
        }

        public BlockInternal[] Blocks = null;

        public int BlockIndex(int x, int y, int z)
        {
            return z * CHUNK_SIZE * CHUNK_SIZE + y * CHUNK_SIZE + x;
        }

        /// <summary>
        /// Performs a raycast.
        /// NOTE: hit.T is always 0.
        /// </summary>
        public bool RayCast(ref Ray ray, float maximumLength, out RayHit hit)
        {
            hit = new RayHit();
            float x = (float)Math.Floor(ray.Position.X);
            float y = (float)Math.Floor(ray.Position.Y);
            float z = (float)Math.Floor(ray.Position.Z);
            float dx = ray.Direction.X;
            float dy = ray.Direction.Y;
            float dz = ray.Direction.Z;
            float stepX = signum(dx);
            float stepY = signum(dy);
            float stepZ = signum(dz);
            float tMaxX = intbound(ray.Position.X, dx);
            float tMaxY = intbound(ray.Position.Y, dy);
            float tMaxZ = intbound(ray.Position.Z, dz);
            float tDeltaX = stepX / dx;
            float tDeltaY = stepY / dy;
            float tDeltaZ = stepZ / dz;
            Vector3 face = new Vector3();
            if (dx == 0 && dy == 0 && dz == 0)
            {
                // Invalid ray?!
                return false;
            }
            while ((stepX > 0 ? x < CHUNK_SIZE : x >= 0) &&
                (stepY > 0 ? y < CHUNK_SIZE : y >= 0) &&
                (stepZ > 0 ? z < CHUNK_SIZE : z >= 0))
            {
                if (!(x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE))
                {
                    BlockInternal bi = Blocks[BlockIndex((int)x, (int)y, (int)z)];
                    if (((Material)bi.BlockMaterial).IsSolid())
                    {
                        // Location offs;
                        // EntityShape es = BlockShapeRegistry.BSD[bi.BlockData].GetShape(out offs);
                        // es.GetCollidableInstance().RayCast(...)
                        // TODO: Trace into the block shape if custom shaped!
                        hit.Normal = face;
                        hit.Location = new Vector3(x, y, z);
                        hit.T = 0;
                        return true;
                    }
                }
                if (tMaxX < tMaxY)
                {
                    if (tMaxX < tMaxZ)
                    {
                        if (tMaxX > maximumLength)
                        {
                            break;
                        }
                        x += stepX;
                        tMaxX += tDeltaX;
                        face.X = -stepX;
                        face.Y = 0;
                        face.Z = 0;
                    }
                    else
                    {
                        if (tMaxZ > maximumLength)
                        {
                            break;
                        }
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
                else
                {
                    if (tMaxY < tMaxZ)
                    {
                        if (tMaxY > maximumLength)
                        {
                            break;
                        }
                        y += stepY;
                        tMaxY += tDeltaY;
                        face.X = 0;
                        face.Y = -stepY;
                        face.Z = 0;
                    }
                    else
                    {
                        if (tMaxZ > maximumLength)
                        {
                            break;
                        }
                        z += stepZ;
                        tMaxZ += tDeltaZ;
                        face.X = 0;
                        face.Y = 0;
                        face.Z = -stepZ;
                    }
                }
            }
            return false;
        }

        float intbound(float s, float ds)
        {
            if (ds < 0)
            {
                return intbound(-s, -ds);
            }
            else
            {
                s = mod(s, 1);
                return (1 - s) / ds;
            }
        }

        float signum(float x)
        {
            return x > 0 ? 1 : x < 0 ? -1 : 0;
        }

        float mod(float value, float modulus)
        {
            return (value % modulus + modulus) % modulus;
        }
    }
}
