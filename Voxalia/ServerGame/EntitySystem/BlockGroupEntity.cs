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
using LiteDB;

namespace Voxalia.ServerGame.EntitySystem
{
    public class BlockGroupEntity: PhysicsEntity
    {
        public int XWidth = 0;

        public int YWidth = 0;

        public int ZWidth = 0;

        public override NetworkEntityType GetNetType()
        {
            return NetworkEntityType.BLOCK_GROUP;
        }

        public override byte[] GetNetData()
        {
            byte[] phys = GetPhysicsNetData();
            int start = phys.Length + 4 + 4 + 4;
            byte[] res = new byte[start + Blocks.Length * 4 + 1 + 4 + 12];
            Utilities.IntToBytes(XWidth).CopyTo(res, phys.Length);
            Utilities.IntToBytes(YWidth).CopyTo(res, phys.Length + 4);
            Utilities.IntToBytes(ZWidth).CopyTo(res, phys.Length + 4 + 4);
            for (int i = 0; i < Blocks.Length; i++)
            {
                Utilities.UshortToBytes(Blocks[i].BlockMaterial).CopyTo(res, start + i * 2);
                res[start + Blocks.Length * 2 + i] = Blocks[i].BlockData;
                res[start + Blocks.Length * 3 + i] = Blocks[i].BlockPaint;
            }
            res[start + Blocks.Length * 4] = (byte)TraceMode;
            Utilities.IntToBytes(Color.ToArgb()).CopyTo(res, start + Blocks.Length * 4 + 1);
            shapeOffs.ToBytes().CopyTo(res, start + Blocks.Length * 4 + 1 + 4);
            scale.ToBytes().CopyTo(res, start + Blocks.Length * 4 + 1 + 4 + 12);
            return res;
        }

        public BGETraceMode TraceMode = BGETraceMode.CONVEX;

        public BlockInternal[] Blocks;

        public Location scale = Location.One;

        public Location shapeOffs;

        public Location Origin;

        public Location rotOffs = Location.Zero;

        public int Angle = 0;

        public System.Drawing.Color Color = System.Drawing.Color.White;

        public override long GetRAMUsage()
        {
            return base.GetRAMUsage() + Blocks.Length * 10;
        }

        public BlockGroupEntity(Location baseloc, BGETraceMode mode, Region tregion, BlockInternal[] blocks, int xwidth, int ywidth, int zwidth, Location torigin = default(Location)) : base(tregion)
        {
            SetMass(blocks.Length * 10f);
            XWidth = xwidth;
            YWidth = ywidth;
            ZWidth = zwidth;
            Blocks = blocks;
            TraceMode = mode;
            Origin = torigin;
            if (TraceMode == BGETraceMode.PERFECT)
            {
                Vector3 shoffs;
                Shape = new MobileChunkShape(new Vector3i(xwidth, ywidth, zwidth), blocks, out shoffs); // TODO: Anything offset related needed here?
                shapeOffs = -new Location(shoffs);
            }
            else
            {
                Shape = CalculateHullShape(out shapeOffs);
                shapeOffs = -shapeOffs;
            }
            SetPosition(baseloc - shapeOffs);
        }
        
        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos);
        }

        public override Location GetPosition()
        {
            return base.GetPosition();
        }

        public override EntityType GetEntityType()
        {
            return EntityType.BLOCK_GROUP;
        }

        public override BsonDocument GetSaveData()
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
        public EntityShape CalculateHullShape(out Location offs)
        {
            List<Vector3> Vertices = new List<Vector3>(XWidth * YWidth * ZWidth);
            for (int x = 0; x < XWidth; x++)
            {
                for (int y = 0; y < YWidth; y++)
                {
                    for (int z = 0; z < ZWidth; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        // TODO: Figure out how to handle solidity here
                        //if (((Material)c.BlockMaterial).GetSolidity() == MaterialSolidity.FULLSOLID)
                        //{
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
                        //}
                    }
                }
            }
            Vector3 center;
            ConvexHullShape chs = new ConvexHullShape(Vertices, out center);
            offs = new Location(center);
            return chs;
        }
    }
}
