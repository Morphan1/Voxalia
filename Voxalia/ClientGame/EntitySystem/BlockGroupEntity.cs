using System;
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

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockGroupEntity : PhysicsEntity
    {
        public int XWidth = 0;

        public int YWidth = 0;

        public int ZWidth = 0;

        public BlockInternal[] Blocks = null;

        public Location shapeOffs;

        public BlockGroupEntity(Region tregion, BlockInternal[] blocks, int xwidth, int ywidth, int zwidth) : base(tregion, true, true)
        {
            SetMass(blocks.Length);
            XWidth = xwidth;
            YWidth = ywidth;
            ZWidth = zwidth;
            Blocks = blocks;
            ConvexEntityShape = CalculateHullShape(out shapeOffs);
            Shape = ConvexEntityShape;
            SetPosition(GetPosition() + shapeOffs);
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
        
        // TODO: Make async!
        public ConvexHullShape CalculateHullShape(out Location offs)
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
                            BlockInternal def = new BlockInternal(0, 0, 0);
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
            ConvexHullShape chs = new ConvexHullShape(Vertices, out center);
            offs = new Location(center);
            return chs;
        }

        public override void Render()
        {
            if (vbo == null)
            {
                return;
            }
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            {
                TheClient.s_fbov.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 2)
            {
                TheClient.s_colormultvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (TheClient.FBOid == 3)
            {
                TheClient.s_transponlyvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TheClient.TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            OpenTK.Matrix4 mat = OpenTK.Matrix4.CreateTranslation(-ClientUtilities.Convert(shapeOffs)) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            float spec = TheClient.Rendering.Specular;
            TheClient.Rendering.SetSpecular(0);
            vbo.Render(false);
            TheClient.Rendering.SetSpecular(spec);
            // TODO: Remove this block
            if (TheClient.FBOid == 1)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_fbo.Bind();
            }
            else if (TheClient.FBOid == 2)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_colormultr.Bind();
            }
            else if (TheClient.FBOid == 3)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                TheClient.s_transponly.Bind();
            }
        }

        public VBO vbo;

        // TODO: Make asyncable!
        public void SetupVBO()
        {
            List<OpenTK.Vector3> Vertices = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector3> Normals = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector3> TexCoords = new List<OpenTK.Vector3>(XWidth * YWidth * ZWidth);
            List<OpenTK.Vector4> Colors = new List<OpenTK.Vector4>(XWidth * YWidth * ZWidth);
            for (int x = 0; x < XWidth; x++)
            {
                for (int y = 0; y < YWidth; y++)
                {
                    for (int z = 0; z < ZWidth; z++)
                    {
                        BlockInternal c = GetBlockAt(x, y, z);
                        if (((Material)c.BlockMaterial).RendersAtAll())
                        {
                            BlockInternal def = new BlockInternal(0, 0, 0);
                            BlockInternal zp = z + 1 < ZWidth ? GetBlockAt(x, y, z + 1) : def;
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : def;
                            BlockInternal yp = y + 1 < YWidth ? GetBlockAt(x, y + 1, z) : def;
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : def;
                            BlockInternal xp = x + 1 < XWidth ? GetBlockAt(x + 1, y, z) : def;
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : def;
                            bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                            bool zps = (((Material)zp.BlockMaterial).IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                            bool zms = (((Material)zm.BlockMaterial).IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                            bool xps = (((Material)xp.BlockMaterial).IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                            bool xms = (((Material)xm.BlockMaterial).IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                            bool yps = (((Material)yp.BlockMaterial).IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                            bool yms = (((Material)ym.BlockMaterial).IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                            Vector3 pos = new Vector3(x, y, z);
                            List<Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                            for (int i = 0; i < vecsi.Count; i++)
                            {
                                Vertices.Add(new OpenTK.Vector3(vecsi[i].X, vecsi[i].Y, vecsi[i].Z));
                            }
                            List<Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                            for (int i = 0; i < normsi.Count; i++)
                            {
                                Normals.Add(new OpenTK.Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z));
                            }
                            List<Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                            for (int i = 0; i < tci.Count; i++)
                            {
                                TexCoords.Add(new OpenTK.Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                            }
                            if (vecsi.Count != normsi.Count || normsi.Count != tci.Count)
                            {
                                SysConsole.Output(OutputType.WARNING, "PROBLEM RENDERING BLOCKGROUP: v:" + vecsi.Count + ",n:" + normsi.Count + ",tci:" + tci.Count);
                            }
                            for (int i = 0; i < vecsi.Count; i++)
                            {
                                Colors.Add(new OpenTK.Vector4(1f));
                            }
                            if (!((Material)c.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[c.BlockData].BackTextureAllowed)
                            {
                                for (int i = vecsi.Count - 1; i >= 0; i--)
                                {
                                    Vertices.Add(new OpenTK.Vector3(vecsi[i].X, vecsi[i].Y, vecsi[i].Z));
                                }
                                for (int i = normsi.Count - 1; i >= 0; i--)
                                {
                                    Normals.Add(new OpenTK.Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z));
                                }
                                for (int i = tci.Count - 1; i >= 0; i--)
                                {
                                    TexCoords.Add(new OpenTK.Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                }
                                // NOTE: Lights!
                                {
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        Colors.Add(new OpenTK.Vector4(1f));
                                    }
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
            vbo.Colors = Colors;
            vbo.Indices = Indices;
            vbo.GenerateVBO();
        }
    }
}
