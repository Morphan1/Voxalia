//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using BEPUphysics.CollisionShapes;
using BEPUutilities;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUutilities.DataStructures;

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

        public static Vector3i[] ReachStarts = new Vector3i[]
        {
            new Vector3i(0, 0, 1),
            new Vector3i(0, 0, 1),
            new Vector3i(0, 0, 1),
            new Vector3i(0, 0, 1),
            new Vector3i(0, 0, 1),
            new Vector3i(1, 0, 0),
            new Vector3i(1, 0, 0),
            new Vector3i(1, 0, 0),
            new Vector3i(0, 1, 0)
        };
        public static Vector3i[] ReachEnds = new Vector3i[]
        {
            new Vector3i(0, 0, -1),
            new Vector3i(-1, 0, 0),
            new Vector3i(1, 0, 0),
            new Vector3i(0, 1, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(-1, 0, 0),
            new Vector3i(0, 1, 0),
            new Vector3i(0, -1, 0),
            new Vector3i(0, -1, 0)
        };

        static Vector3i[] MoveDirs = new Vector3i[] { new Vector3i(-1, 0, 0), new Vector3i(1, 0, 0),
            new Vector3i(0, -1, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, -1), new Vector3i(0, 0, 1) };

        public bool CanReach(Vector3i snorm, Vector3i enorm)
        {
            // Probably a better way to do this
            Vector3i low;
            Vector3i high;
            if (snorm.X == -1)
            {
                low = new Vector3i(0, 0, 0);
                high = new Vector3i(0, CHUNK_SIZE, CHUNK_SIZE);
            }
            else if (snorm.X == 1)
            {
                low = new Vector3i(CHUNK_SIZE, 0, 0);
                high = new Vector3i(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
            }
            else if (snorm.Y == -1)
            {
                low = new Vector3i(0, 0, 0);
                high = new Vector3i(CHUNK_SIZE, 0, CHUNK_SIZE);
            }
            else if (snorm.Y == 1)
            {
                low = new Vector3i(0, CHUNK_SIZE, 0);
                high = new Vector3i(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
            }
            else if (snorm.Z == -1)
            {
                low = new Vector3i(0, 0, 0);
                high = new Vector3i(CHUNK_SIZE, CHUNK_SIZE, 0);
            }
            else
            {
                low = new Vector3i(0, 0, CHUNK_SIZE);
                high = new Vector3i(CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE);
            }
            for (int x = low.X; x <= high.X; x++)
            {
                for (int y = low.Y; y <= high.Y; y++)
                {
                    for (int z = low.Z; z <= high.Z; z++)
                    {
                        if (PointCanReach(new Vector3i(x, y, z), enorm))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public bool PointCanReach(Vector3i p, Vector3i enorm)
        {
            bool[] traced = new bool[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
            Queue<Vector3i> toTrace = new Queue<Vector3i>();
            toTrace.Enqueue(p);
            while (toTrace.Count > 0)
            {
                Vector3i tp = toTrace.Dequeue();
                traced[BlockIndex(tp.X, tp.Y, tp.Z)] = true;
                for (int i = 0; i < MoveDirs.Length; i++)
                {
                    Vector3i np = tp + MoveDirs[i];
                    if (np.X < 0 || np.Y < 0 || np.Z < 0 || np.X >= CHUNK_SIZE || np.Y >= CHUNK_SIZE || np.Z >= CHUNK_SIZE)
                    {
                        if ((np.X < 0 && enorm.X == -1) || (np.X >= CHUNK_SIZE && enorm.X == 1)
                            || (np.Y < 0 && enorm.Y == -1) || (np.Y >= CHUNK_SIZE && enorm.Y == 1)
                            || (np.Y < 0 && enorm.Y == -1) || (np.Y >= CHUNK_SIZE && enorm.Y == 1))
                        {
                            return true;
                        }
                        continue;
                    }
                    if (!traced[BlockIndex(tp.X, tp.Y, tp.Z)] && !Blocks[BlockIndex(np.X, np.Y, np.Z)].IsOpaque())
                    {
                        toTrace.Enqueue(np);
                    }
                }
            }
            return false;
        }

        public ConvexShape ShapeAt(int x, int y, int z, out Vector3 offs)
        {
            if (x < 0 || y < 0 || z < 0 || x >= CHUNK_SIZE || y >= CHUNK_SIZE || z >= CHUNK_SIZE)
            {
                offs = new Vector3(double.NaN, double.NaN, double.NaN);
                return null;
            }
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
        public void GetOverlaps(Vector3 gridPosition, BoundingBox boundingBox, ref QuickList<Vector3i> overlaps)
        {
            BoundingBox b2 = new BoundingBox();
            Vector3.Subtract(ref boundingBox.Min, ref gridPosition, out b2.Min);
            Vector3.Subtract(ref boundingBox.Max, ref gridPosition, out b2.Max);
            var min = new Vector3i
            {
                X = Math.Max(0, (int)b2.Min.X),
                Y = Math.Max(0, (int)b2.Min.Y),
                Z = Math.Max(0, (int)b2.Min.Z)
            };
            var max = new Vector3i
            {
                X = Math.Min(CHUNK_SIZE - 1, (int)b2.Max.X),
                Y = Math.Min(CHUNK_SIZE - 1, (int)b2.Max.Y),
                Z = Math.Min(CHUNK_SIZE - 1, (int)b2.Max.Z)
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
    }
}
