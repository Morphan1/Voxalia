using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using Voxalia.Shared;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;
using Voxalia.Shared.Collision;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockGroupEntity: PhysicsEntity
    {
        public int XWidth = 0;

        public int YWidth = 0;

        public int ZWidth = 0;

        public BGETraceMode TraceMode = BGETraceMode.CONVEX;

        public BlockInternal[] Blocks = null;
        
        public BlockGroupEntity(Location baseloc, BGETraceMode mode, Region tregion, BlockInternal[] blocks, int xwidth, int ywidth, int zwidth) : base(tregion)
        {
            SetMass(blocks.Length);
            SetPosition(baseloc);
            XWidth = xwidth;
            YWidth = ywidth;
            ZWidth = zwidth;
            Blocks = blocks;
            Location shapeOffs;
            TraceMode = mode;
            ConvexEntityShape = (ConvexShape)CalculateHullShape(BGETraceMode.CONVEX, out shapeOffs);
            if (TraceMode == BGETraceMode.PERFECT)
            {
                Shape = new MobileChunkShape(new Vector3i(xwidth, ywidth, zwidth), blocks);
            }
            else
            {
                Shape = ConvexEntityShape;
            }
            SetPosition(GetPosition() + shapeOffs);
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BLOCK_GROUP;
        }

        public override byte[] GetSaveBytes()
        {
            // TODO: Save properly!
            return null;
        }

        public int BlockIndex(int x, int y, int z)
        {
            return z * YWidth * XWidth + y * XWidth + x;
        }

        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return Blocks[BlockIndex(x, y, z)];
        }
        
        // TODO: Async?
        public EntityShape CalculateHullShape(BGETraceMode mode, out Location offs)
        {
            List<Vector3> Vertices = new List<Vector3>(XWidth * YWidth * ZWidth);
            for (int x = 0; x < XWidth; x++)
            {
                for (int y = 0; y < YWidth; y++)
                {
                    for (int z = 0; z < ZWidth; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        if (((Material)c.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID)
                        {
                            BlockInternal def = new BlockInternal(0, 0, 0, 0);
                            BlockInternal zp = z + 1 < ZWidth ? GetBlockAt(x, y, z + 1) : def;
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : def;
                            BlockInternal yp = y + 1 < YWidth ? GetBlockAt(x, y + 1, z) : def;
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : def;
                            BlockInternal xp = x + 1 < XWidth ? GetBlockAt(x + 1, y, z) : def;
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : def;
                            bool zps = ((Material)zp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                            bool zms = ((Material)zm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                            bool xps = ((Material)xp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                            bool xms = ((Material)xm.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                            bool yps = ((Material)yp.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                            bool yms = ((Material)ym.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                            Vector3 pos = new Vector3(x, y, z);
                            List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            Vertices.AddRange(vecsi);
                        }
                    }
                }
            }
            Vector3 center;
            if (mode == BGETraceMode.PERFECT)
            {
                int[] indices = new int[Vertices.Count];
                for (int i = 0; i < Vertices.Count; i++)
                {
                    indices[i] = i;
                }
                MobileMeshShape mesh = new MobileMeshShape(Vertices.ToArray(), indices, AffineTransform.Identity, MobileMeshSolidity.DoubleSided, out center);
                offs = new Location(center);
                return mesh;
            }
            else
            {
                ConvexHullShape chs = new ConvexHullShape(Vertices, out center);
                offs = new Location(center);
                return chs;
            }
        }
    }
}
