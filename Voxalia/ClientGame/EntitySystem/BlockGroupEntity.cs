﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared;
using BEPUphysics;
using BEPUutilities;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockGroupEntity : PhysicsEntity
    {
        public int XWidth = 0;

        public int YWidth = 0;

        public int ZWidth = 0;

        public BlockInternal[] Blocks = null;

        public BGETraceMode TraceMode = BGETraceMode.CONVEX;

        public Location shapeOffs;

        public System.Drawing.Color Color = System.Drawing.Color.White;

        public BlockGroupEntity(Region tregion, BGETraceMode mode, BlockInternal[] blocks, int xwidth, int ywidth, int zwidth, Location sOffs) : base(tregion, true, true)
        {
            SetMass(blocks.Length);
            XWidth = xwidth;
            YWidth = ywidth;
            ZWidth = zwidth;
            Blocks = blocks;
            TraceMode = mode;
            if (TraceMode == BGETraceMode.PERFECT)
            {
                Shape = new MobileChunkShape(new Vector3i(xwidth, ywidth, zwidth), blocks);
            }
            else
            {
                Shape = CalculateHullShape(out shapeOffs);
            }
            shapeOffs = sOffs;
            SetPosition(GetPosition());
        }

        public override void SpawnBody()
        {
            SetupVBO();
            base.SpawnBody();
        }

        public override void DestroyBody()
        {
            vbo.Destroy();
            base.DestroyBody();
        }

        public int BlockIndex(int x, int y, int z)
        {
            return z * YWidth * XWidth + y * XWidth + x;
        }

        public BlockInternal GetBlockAt(int x, int y, int z)
        {
            return Blocks[BlockIndex(x, y, z)];
        }

        public bool pActive = false;

        public double deltat = 0;

        // TODO: Make async!?
        public EntityShape CalculateHullShape(out Location offs)
        {
            List<Vector3> Vertices = new List<Vector3>(XWidth * YWidth * ZWidth);
            BlockInternal def = new BlockInternal(0, 0, 0, 0);
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

        public override void SetPosition(Location pos)
        {
            base.SetPosition(pos);
        }

        public override Location GetPosition()
        {
            return base.GetPosition();
        }

        public override void Render()
        {
            if (vbo == null)
            {
                return;
            }
            TheClient.SetVox();
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateTranslation(ClientUtilities.Convert(shapeOffs)) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetColor(Color);
            vbo.Render(false);
            TheClient.Rendering.SetColor(Color4.White);
        }

        public VBO vbo;

        // TODO: Make asyncable!
        public void SetupVBO()
        {
            List<OpenTK.Vector3> Vertices = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector3> Normals = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector3> TexCoords = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector4> Colrs = new List<OpenTK.Vector4>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector4> TCOLs = new List<OpenTK.Vector4>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector3> Tangs = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            for (int x = 0; x < XWidth; x++)
            {
                for (int y = 0; y < YWidth; y++)
                {
                    for (int z = 0; z < ZWidth; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        if (((Material)c.BlockMaterial).RendersAtAll())
                        {
                            BlockInternal def = new BlockInternal(0, 0, 0, 0);
                            BlockInternal zp = z + 1 < ZWidth ? GetBlockAt(x, y, z + 1) : def;
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : def;
                            BlockInternal yp = y + 1 < YWidth ? GetBlockAt(x, y + 1, z) : def;
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : def;
                            BlockInternal xp = x + 1 < XWidth ? GetBlockAt(x + 1, y, z) : def;
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : def;
                            bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                            bool zps = (zp.IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                            bool zms = (zm.IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                            bool xps = (xp.IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                            bool xms = (xm.IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                            bool yps = (yp.IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                            bool yms = (ym.IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                            Vector3 pos = new Vector3(x, y, z);
                            List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                            List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                            int vertcount = Vertices.Count;
                            for (int i = 0; i < vecsi.Count; i++)
                            {
                                // TODO: is PosMultiplier used correctly here?
                                OpenTK.Vector3 vt = new OpenTK.Vector3(vecsi[i].X, vecsi[i].Y, vecsi[i].Z);
                                Vertices.Add(vt);
                                OpenTK.Vector3 nt = new OpenTK.Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z);
                                Normals.Add(nt);
                                TexCoords.Add(new OpenTK.Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                Colrs.Add(new OpenTK.Vector4(1, 1, 1, 1));
                                TCOLs.Add(TheClient.Rendering.AdaptColor(vt, Colors.ForByte(c.BlockPaint)));
                            }
                            for (int i = 0; i < vecsi.Count; i += 3)
                            {
                                int basis = vertcount + i;
                                OpenTK.Vector3 v1 = Vertices[basis];
                                OpenTK.Vector3 dv1 = Vertices[basis + 1] - v1;
                                OpenTK.Vector3 dv2 = Vertices[basis + 2] - v1;
                                OpenTK.Vector3 t1 = TexCoords[basis];
                                OpenTK.Vector3 dt1 = TexCoords[basis + 1] - t1;
                                OpenTK.Vector3 dt2 = TexCoords[basis + 2] - t1;
                                OpenTK.Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) * 1f / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                                OpenTK.Vector3 normal = Normals[basis];
                                tangent = (tangent - normal * OpenTK.Vector3.Dot(normal, tangent)).Normalized();
                                Tangs.Add(tangent);
                                Tangs.Add(tangent);
                                Tangs.Add(tangent);
                            }
                            if (!c.IsOpaque() && BlockShapeRegistry.BSD[c.BlockData].BackTextureAllowed)
                            {
                                int tf = Colrs.Count - vecsi.Count;
                                for (int i = vecsi.Count - 1; i >= 0; i--)
                                {
                                    Vertices.Add(new OpenTK.Vector3(vecsi[i].X, vecsi[i].Y, vecsi[i].Z));
                                    int tx = tf + i;
                                    Colrs.Add(Colrs[tx]);
                                    TCOLs.Add(TCOLs[tx]);
                                    Normals.Add(new OpenTK.Vector3(-normsi[i].X, -normsi[i].Y, -normsi[i].Z));
                                    TexCoords.Add(new OpenTK.Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                }
                            }
                        }
                    }
                }
            }
            if (vbo != null)
            {
                vbo.Destroy();
                vbo = null;
            }
            if (Vertices.Count == 0)
            {
                return;
            }
            List<uint> Indices = new List<uint>(Vertices.Count);
            for (uint i = 0; i < Vertices.Count; i++)
            {
                Indices.Add(i);
            }
            vbo = new VBO();
            vbo.Vertices = Vertices;
            vbo.Normals = Normals;
            vbo.TexCoords = TexCoords;
            vbo.Colors = Colrs;
            vbo.TCOLs = TCOLs;
            vbo.Tangents = Tangs;
            vbo.Indices = Indices;
            vbo.GenerateVBO();
        }
    }
}
