using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public VBO _VBO = null;

        public void CreateVBO(Action callback = null)
        {
            if (rendering != null)
            {
                ASyncScheduleItem item = OwningRegion.TheClient.Schedule.AddASyncTask(() => VBOHInternal(callback));
                rendering = rendering.ReplaceOrFollowWith(item);
            }
            else
            {
                rendering = OwningRegion.TheClient.Schedule.StartASyncTask(() => VBOHInternal(callback));
            }
        }

        public ASyncScheduleItem rendering = null;
        
        public static Vector3i[] dirs = new Vector3i[] { new Vector3i(1, 0, 0), new Vector3i(0, 1, 0), new Vector3i(0, 0, 1), new Vector3i(1, 1, 0), new Vector3i(0, 1, 1), new Vector3i(1, 0, 1),
        new Vector3i(-1, 1, 0), new Vector3i(0, -1, 1), new Vector3i(-1, 0, 1), new Vector3i(1, 1, 1), new Vector3i(-1, 1, 1), new Vector3i(1, -1, 1), new Vector3i(1, 1, -1), new Vector3i(-1, -1, 1),
        new Vector3i(-1, 1, -1), new Vector3i(1, -1, -1) };

        void VBOHInternal(Action callback)
        {
            OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
            {
                OwningRegion.TheClient.ChunksRenderingCurrently++;
            });
            try
            {
                List<Vector3> Vertices = new List<Vector3>(CSize * CSize * CSize * 6); // TODO: Make this an array?
                List<Vector3> TCoords = new List<Vector3>(CSize * CSize * CSize * 6);
                List<Vector3> Norms = new List<Vector3>(CSize * CSize * CSize * 6);
                List<Vector4> Cols = new List<Vector4>(CSize * CSize * CSize * 6);
                List<Vector4> TCols = new List<Vector4>(CSize * CSize * CSize * 6);
                Vector3 ppos = ClientUtilities.Convert(WorldPosition * 30);
                bool light = OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB;
                Chunk c_zp = OwningRegion.GetChunk(WorldPosition + new Location(0, 0, 1));
                Chunk c_zm = OwningRegion.GetChunk(WorldPosition + new Location(0, 0, -1));
                Chunk c_yp = OwningRegion.GetChunk(WorldPosition + new Location(0, 1, 0));
                Chunk c_ym = OwningRegion.GetChunk(WorldPosition + new Location(0, -1, 0));
                Chunk c_xp = OwningRegion.GetChunk(WorldPosition + new Location(1, 0, 0));
                Chunk c_xm = OwningRegion.GetChunk(WorldPosition + new Location(-1, 0, 0));
                BlockInternal t_air = new BlockInternal(0, 0, 0, 0);
                for (int x = 0; x < CSize; x++)
                {
                    for (int y = 0; y < CSize; y++)
                    {
                        for (int z = 0; z < CSize; z++)
                        {
                            BlockInternal c = GetBlockAt(x, y, z);
                            if (((Material)c.BlockMaterial).RendersAtAll())
                            {
                                // TODO: Handle ALL blocks against the surface when low-LOD
                                BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : (c_zp == null ? t_air : c_zp.GetBlockAt(x, y, z + 1 - CSize));
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : (c_zm == null ? t_air : c_zm.GetBlockAt(x, y, z - 1 + CSize));
                                BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : (c_yp == null ? t_air : c_yp.GetBlockAt(x, y + 1 - CSize, z));
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : (c_ym == null ? t_air : c_ym.GetBlockAt(x, y - 1 + CSize, z));
                                BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : (c_xp == null ? t_air : c_xp.GetBlockAt(x + 1 - CSize, y, z));
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : (c_xm == null ? t_air : c_xm.GetBlockAt(x - 1 + CSize, y, z));
                                bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                                bool zps = (zp.IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial && zp.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                                bool zms = (zm.IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial && zm.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                                bool xps = (xp.IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial && xp.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                                bool xms = (xm.IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial && xm.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                                bool yps = (yp.IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial && yp.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                                bool yms = (ym.IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial && ym.BlockPaint == c.BlockPaint))) && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                                BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(x, y, z);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    // TODO: is PosMultiplier used correctly here?
                                    Vector3 vt = new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z);
                                    Vertices.Add(vt);
                                    Vector3 nt = new Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z);
                                    Norms.Add(nt);
                                    TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                    Location lcol = OwningRegion.GetLightAmount(ClientUtilities.Convert(vt), ClientUtilities.Convert(nt), this);
                                    Cols.Add(new Vector4((float)lcol.X, (float)lcol.Y, (float)lcol.Z, 1));
                                    System.Drawing.Color tcol = Colors.ForByte(c.BlockPaint);
                                    if (tcol.A == 0)
                                    {
                                        // TODO: Better picker thingy... 3d noise?
                                        Random urand = new Random((int)(vt.X + vt.Y + vt.Z + ppos.X + ppos.Y + ppos.Z));
                                        TCols.Add(new Vector4((float)urand.NextDouble(), (float)urand.NextDouble(), (float)urand.NextDouble(), 1f));
                                    }
                                    else
                                    {
                                        TCols.Add(new Vector4(tcol.R / 255f, tcol.G / 255f, tcol.B / 255f, tcol.A / 255f));
                                    }
                                }
                                if (!c.IsOpaque() && BlockShapeRegistry.BSD[c.BlockData].BackTextureAllowed)
                                {
                                    int tf = Cols.Count - vecsi.Count;
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        Vertices.Add(new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z));
                                        int tx = tf + i;
                                        Cols.Add(Cols[tx]);
                                        TCols.Add(TCols[tx]);
                                        Norms.Add(new Vector3(-normsi[i].X, -normsi[i].Y, -normsi[i].Z));
                                        TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                    }
                                }
                            }
                        }
                    }
                }
                if (Vertices.Count == 0)
                {
                    OwningRegion.TheClient.ChunksRenderingCurrently--;
                    OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                    {
                        if (_VBO != null)
                        {
                            VBO tV = _VBO;
                            lock (OwningRegion.TheClient.vbos)
                            {
                                if (OwningRegion.TheClient.vbos.Length < 120)
                                {
                                    OwningRegion.TheClient.vbos.Push(tV);
                                }
                                else
                                {
                                    tV.Destroy();
                                }
                            }
                        }
                    });
                    _VBO = null;
                    if (DENIED)
                    {
                        return;
                    }
                    if (callback != null)
                    {
                        callback.Invoke();
                    }
                    return;
                }
                uint[] inds = new uint[Vertices.Count];
                for (uint i = 0; i < Vertices.Count; i++)
                {
                    inds[i] = i;
                }
                VBO tVBO;
                lock (OwningRegion.TheClient.vbos)
                {
                    if (OwningRegion.TheClient.vbos.Length > 0)
                    {
                        tVBO = OwningRegion.TheClient.vbos.Pop();
                    }
                    else
                    {
                        tVBO = new VBO();
                        tVBO.BufferMode = OpenTK.Graphics.OpenGL4.BufferUsageHint.StreamDraw;
                    }
                }
                tVBO.indices = inds;
                tVBO.Vertices = Vertices;
                tVBO.Normals = Norms;
                tVBO.TexCoords = TCoords;
                tVBO.Colors = Cols;
                tVBO.TCOLs = TCols;
                tVBO.BoneWeights = null;
                tVBO.BoneIDs = null;
                tVBO.BoneWeights2 = null;
                tVBO.BoneIDs2 = null;
                tVBO.oldvert();
                OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    if (_VBO != null)
                    {
                        VBO tV = _VBO;
                        lock (OwningRegion.TheClient.vbos)
                        {
                            if (OwningRegion.TheClient.vbos.Length < 120)
                            {
                                OwningRegion.TheClient.vbos.Push(tV);
                            }
                            else
                            {
                                tV.Destroy();
                            }
                        }
                    }
                    OwningRegion.TheClient.ChunksRenderingCurrently--;
                    if (DENIED)
                    {
                        return;
                    }
                    _VBO = tVBO;
                    tVBO.GenerateOrUpdate();
                    tVBO.CleanLists();
                    if (callback != null)
                    {
                        callback.Invoke();
                    }
                });
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Generating ChunkVBO...: " + ex.ToString());
            }
        }
        
        public BlockInternal SpecialGetBlockAt(int x, int y, int z)
        {
            if (x >= 0 && y >= 0 && z >= 0 && x < CSize && y < CSize && z < CSize)
            {
                return GetBlockAt(x, y, z);
            }
            return OwningRegion.GetBlockInternal(WorldPosition * 30.0 + new Location(x, y, z));
        }

        public void Render()
        {
            if (_VBO != null && _VBO.generated)
            {
                _VBO.Render(OwningRegion.TheClient.RenderTextures);
            }
        }
    }
}
