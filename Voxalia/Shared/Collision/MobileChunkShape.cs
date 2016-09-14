using BEPUphysics.CollisionShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using System;
using BEPUutilities.DataStructures;

namespace Voxalia.Shared.Collision
{
    public class MobileChunkShape : EntityShape
    {
        public Vector3i ChunkSize;

        public MobileChunkShape(Vector3i csize, BlockInternal[] blocks, out Vector3 center)
        {
            Matrix3x3 boxMat = new BoxShape(csize.X, csize.Y, csize.Z).VolumeDistribution;
            ChunkSize = csize;
            Blocks = blocks;
            double weightInv = 1f / blocks.Length;
            center = new Vector3(csize.X / 2f, csize.Y / 2f, csize.Z / 2f);
            // TODO: More accurately get center of weight based on which blocks are solid or not!?
            Matrix3x3 volumeDistribution = new Matrix3x3();
            RigidTransform transform = new RigidTransform(center);
            Matrix3x3 contribution;
            CompoundShape.TransformContribution(ref transform, ref center, ref boxMat, blocks.Length, out contribution);
            Matrix3x3.Add(ref volumeDistribution, ref contribution, out volumeDistribution);
            Matrix3x3.Multiply(ref volumeDistribution, weightInv, out volumeDistribution);
            UpdateEntityShapeVolume(new EntityShapeVolumeDescription() { Volume = csize.X * csize.Y * csize.Z, VolumeDistribution = volumeDistribution });
            Center = center;
        }

        public Vector3 Center;

        public BlockInternal[] Blocks = null;

        public int BlockIndex(int x, int y, int z)
        {
            return z * ChunkSize.Y * ChunkSize.X + y * ChunkSize.X + x;
        }

        public ConvexShape ShapeAt(int x, int y, int z, out Vector3 offs)
        {
            Location loffs;
            BlockInternal bi = Blocks[BlockIndex(x, y, z)];
            ConvexShape shape = (ConvexShape)BlockShapeRegistry.BSD[bi.BlockData].GetShape(bi.Damage, out loffs, false);
            offs = loffs.ToBVector();
            return shape;
        }

        public bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweepnorm, double slen, MaterialSolidity solidness, out RayHit hit)
        {
            BoundingBox bb;
            RigidTransform rot = new RigidTransform(Vector3.Zero, startingTransform.Orientation);
            castShape.GetBoundingBox(ref rot, out bb);
            double adv = 0.1f;
            double max = slen + adv;
            bool gotOne = false;
            RayHit BestRH = default(RayHit);
            Vector3 sweep = sweepnorm * slen;
            for (double f = 0; f < max; f += adv)
            {
                Vector3 c = startingTransform.Position + sweepnorm * f;
                int mx = (int)Math.Ceiling(c.X + bb.Max.X);
                for (int x = (int)Math.Floor(c.X + bb.Min.X); x <= mx; x++)
                {
                    if (x < 0 || x >= ChunkSize.X)
                    {
                        continue;
                    }
                    int my = (int)Math.Ceiling(c.Y + bb.Max.Y);
                    for (int y = (int)Math.Floor(c.Y + bb.Min.Y); y <= my; y++)
                    {
                        if (y < 0 || y >= ChunkSize.Y)
                        {
                            continue;
                        }
                        int mz = (int)Math.Ceiling(c.Z + bb.Max.Z);
                        for (int z = (int)Math.Floor(c.Z + bb.Min.Z); z <= mz; z++)
                        {
                            if (z < 0 || z >= ChunkSize.Z)
                            {
                                continue;
                            }
                            BlockInternal bi = Blocks[BlockIndex(x, y, z)];
                            if (solidness.HasFlag(((Material)bi.BlockMaterial).GetSolidity()))
                            {
                                Location offs;
                                EntityShape es = BlockShapeRegistry.BSD[bi.BlockData].GetShape(bi.Damage, out offs, false);
                                if (es == null)
                                {
                                    continue;
                                }
                                Vector3 adj = new Vector3(x + (double)offs.X, y + (double)offs.Y, z + (double)offs.Z);
                                EntityCollidable coll = es.GetCollidableInstance();
                                //coll.LocalPosition = adj;
                                RigidTransform rt = new RigidTransform(Vector3.Zero, Quaternion.Identity);
                                coll.LocalPosition = Vector3.Zero;
                                coll.WorldTransform = rt;
                                coll.UpdateBoundingBoxForTransform(ref rt);
                                RayHit rhit;
                                RigidTransform adjusted = new RigidTransform(startingTransform.Position - adj, startingTransform.Orientation);
                                bool b = coll.ConvexCast(castShape, ref adjusted, ref sweep, out rhit);
                                if (b && (!gotOne || rhit.T * slen < BestRH.T) && rhit.T >= 0)
                                {
                                    gotOne = true;
                                    BestRH = rhit;
                                    BestRH.Location += adj;
                                    BestRH.T *= slen; // TODO: ???
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
            hit = new RayHit() { Location = startingTransform.Position + sweep, Normal = new Vector3(0, 0, 0), T = slen };
            return false;
        }

        BoxShape RayCastShape = new BoxShape(0.1f, 0.1f, 0.1f);

        public bool RayCast(ref Ray ray, double maximumLength, MaterialSolidity solidness, out RayHit hit)
        {
            // TODO: Original special ray code!
            RigidTransform rt = new RigidTransform(ray.Position, Quaternion.Identity);
            Vector3 sweep = ray.Direction;
            return ConvexCast(RayCastShape, ref rt, ref sweep, maximumLength, solidness, out hit);
        }

        // TODO: Optimize me!
        public void GetOverlaps(ref RigidTransform transform, BoundingBox boundingBox, ref QuickList<Vector3i> overlaps)
        {
            Vector3 tmin, tmax;
            RigidTransform.TransformByInverse(ref boundingBox.Min, ref transform, out tmin);
            RigidTransform.TransformByInverse(ref boundingBox.Max, ref transform, out tmax);
            BoundingBox b2 = new BoundingBox(Vector3.Min(tmin, tmax), Vector3.Max(tmin, tmax));
            var min = new Vector3i
            {
                X = Math.Max(0, (int)b2.Min.X),
                Y = Math.Max(0, (int)b2.Min.Y),
                Z = Math.Max(0, (int)b2.Min.Z)
            };
            var max = new Vector3i
            {
                X = Math.Min(ChunkSize.X - 1, (int)b2.Max.X),
                Y = Math.Min(ChunkSize.Y - 1, (int)b2.Max.Y),
                Z = Math.Min(ChunkSize.Z - 1, (int)b2.Max.Z)
            };
            for (int x = min.X; x <= max.X; x++)
            {
                for (int y = min.Y; y <= max.Y; y++)
                {
                    for (int z = min.Z; z <= max.Z; z++)
                    {
                        if (Blocks[BlockIndex(x, y, z)].Material.GetSolidity() == MaterialSolidity.FULLSOLID)
                        {
                            overlaps.Add(new Vector3i { X = x, Y = y, Z = z });
                        }
                    }
                }
            }
        }

        public override EntityCollidable GetCollidableInstance()
        {
            return new MobileChunkCollidable(this);
        }

        public override void GetBoundingBox(ref RigidTransform transform, out BoundingBox boundingBox)
        {
            // Lazily overestimate!
            // TODO: More tightly wrapped around the actual shape.
            Vector3 maxbase = new Vector3(ChunkSize.X, ChunkSize.Y, ChunkSize.Z) * 2f;
            boundingBox = new BoundingBox(transform.Position - maxbase, transform.Position + maxbase);
        }
    }
}
