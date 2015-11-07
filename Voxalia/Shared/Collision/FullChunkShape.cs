using BEPUphysics.CollisionShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
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
        
        public bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweepnorm, float slen, out RayHit hit)
        {
            BoundingBox bb;
            RigidTransform rot = new RigidTransform(Vector3.Zero, startingTransform.Orientation);
            castShape.GetBoundingBox(ref rot, out bb);
            float adv = 0.1f;
            float max = slen + adv;
            bool gotOne = false;
            RayHit BestRH = default(RayHit);
            Vector3 sweep = sweepnorm * slen;
            for (float f = 0; f < max; f += adv)
            {
                Vector3 c = startingTransform.Position + sweepnorm * f;
                int mx = (int)Math.Ceiling(c.X + bb.Max.X);
                for (int x = (int)Math.Floor(c.X + bb.Min.X); x <= mx; x++)
                {
                    if (x < 0 || x >= CHUNK_SIZE)
                    {
                        continue;
                    }
                    int my = (int)Math.Ceiling(c.Y + bb.Max.Y);
                    for (int y = (int)Math.Floor(c.Y + bb.Min.Y); y <= my; y++)
                    {
                        if (y < 0 || y >= CHUNK_SIZE)
                        {
                            continue;
                        }
                        int mz = (int)Math.Ceiling(c.Z + bb.Max.Z);
                        for (int z = (int)Math.Floor(c.Z + bb.Min.Z); z <= mz; z++)
                        {
                            if (z < 0 || z >= CHUNK_SIZE)
                            {
                                continue;
                            }
                            BlockInternal bi = Blocks[BlockIndex(x, y, z)];
                            if (((Material)bi.BlockMaterial).IsSolid())
                            {
                                Location offs;
                                EntityShape es = BlockShapeRegistry.BSD[bi.BlockData].GetShape(out offs);
                                if (es == null)
                                {
                                    continue;
                                }
                                Vector3 adj = new Vector3(x + (float)offs.X, y + (float)offs.Y, z + (float)offs.Z);
                                EntityCollidable coll = es.GetCollidableInstance();
                                //coll.LocalPosition = adj;
                                RigidTransform rt = new RigidTransform(Vector3.Zero, Quaternion.Identity);
                                coll.LocalPosition = Vector3.Zero;
                                coll.WorldTransform = rt;
                                coll.UpdateBoundingBoxForTransform(ref rt);
                                RayHit rhit;
                                RigidTransform adjusted = new RigidTransform(startingTransform.Position - adj, startingTransform.Orientation);
                                bool b = coll.ConvexCast(castShape, ref adjusted, ref sweep, out rhit);
                                if (b && (!gotOne || rhit.T < BestRH.T) && rhit.T <= 1 && rhit.T >= 0)
                                {
                                    gotOne = true;
                                    BestRH = rhit;
                                    BestRH.Location += adj;
                                    BestRH.Normal = -BestRH.Normal; // TODO: WHY?!
                                }
                            }
                        }
                    }
                }
                if (gotOne)
                {
                    hit = BestRH;
                    return true;
                }
            }
            hit = new RayHit() { Location = startingTransform.Position + sweep, Normal = new Vector3(0, 0, 0), T = 1 };
            return false;
        }

        /// <summary>
        /// Performs a raycast.
        /// NOTE: hit.T is always 0.
        /// </summary>
        public bool RayCast(ref Ray ray, float maximumLength, out RayHit hit)
        {
            // TODO: Original special ray code!
            RigidTransform rt = new RigidTransform(ray.Position, Quaternion.Identity);
            Vector3 sweep = ray.Direction;
            return ConvexCast(new BoxShape(0.1f, 0.1f, 0.1f), ref rt, ref sweep, maximumLength, out hit);
        }
    }
}
