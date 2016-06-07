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
        
        /// <summary>
        /// Sync only.
        /// </summary>
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

        BlockInternal GetMostSolid(Chunk c, int x, int y, int z)
        {
            if (c.PosMultiplier == PosMultiplier)
            {
                return c.GetBlockAt(x, y, z);
            }
            // TODO: better method here...
            return BlockInternal.AIR;
        }

        void VBOHInternal(Action callback)
        {
            try
            {
                Object locky = new Object();
                ChunkRenderHelper rh;
                lock (locky)
                {
                    rh = new ChunkRenderHelper();
                }
                if (DENIED)
                {
                    return;
                }
                OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    OwningRegion.TheClient.ChunksRenderingCurrently++;
                });
                Vector3 ppos = ClientUtilities.Convert(WorldPosition.ToLocation() * CHUNK_SIZE);
                //bool light = OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB;
                Chunk c_zp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, 1));
                Chunk c_zm = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, -1));
                Chunk c_yp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 1, 0));
                Chunk c_ym = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, -1, 0));
                Chunk c_xp = OwningRegion.GetChunk(WorldPosition + new Vector3i(1, 0, 0));
                Chunk c_xm = OwningRegion.GetChunk(WorldPosition + new Vector3i(-1, 0, 0));
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
                                BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : (c_zp == null ? t_air : GetMostSolid(c_zp, x, y, z + 1 - CSize));
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : (c_zm == null ? t_air : GetMostSolid(c_zm, x, y, z - 1 + CSize));
                                BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : (c_yp == null ? t_air : GetMostSolid(c_yp, x, y + 1 - CSize, z));
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : (c_ym == null ? t_air : GetMostSolid(c_ym, x, y - 1 + CSize, z));
                                BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : (c_xp == null ? t_air : GetMostSolid(c_xp, x + 1 - CSize, y, z));
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : (c_xm == null ? t_air : GetMostSolid(c_xm, x - 1 + CSize, y, z));
                                bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                                bool pMatters = !c.IsOpaque();
                                bool zps = (zp.IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial && (pMatters || zp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                                bool zms = (zm.IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial && (pMatters || zm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                                bool xps = (xp.IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial && (pMatters || xp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                                bool xms = (xm.IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial && (pMatters || xm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                                bool yps = (yp.IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial && (pMatters || yp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                                bool yms = (ym.IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial && (pMatters || ym.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                                BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(x, y, z);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                                KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>> ths = !c.BlockShareTex ? default(KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>>) :
                                    BlockShapeRegistry.BSD[c.BlockData].GetStretchData(pos, vecsi, xp, xm, yp, ym, zp, zm, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    Vector3 vt = new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z);
                                    rh.Vertices.Add(vt);
                                    Vector3 nt = new Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z);
                                    rh.Norms.Add(nt);
                                    rh.TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                    Location lcol = OwningRegion.GetLightAmount(ClientUtilities.Convert(vt), ClientUtilities.Convert(nt), this);
                                    rh.Cols.Add(new Vector4((float)lcol.X, (float)lcol.Y, (float)lcol.Z, 1));
                                    rh.TCols.Add(OwningRegion.TheClient.Rendering.AdaptColor(vt, Colors.ForByte(c.BlockPaint)));
                                    if (ths.Key != null)
                                    {
                                        rh.THVs.Add(new Vector4(ths.Key[i].X, ths.Key[i].Y, ths.Key[i].Z, ths.Key[i].W));
                                        rh.THWs.Add(new Vector4(ths.Value[i].X, ths.Value[i].Y, ths.Value[i].Z, ths.Value[i].W));
                                    }
                                    else
                                    {
                                        rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                        rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                    }
                                }
                                if (!c.IsOpaque() && BlockShapeRegistry.BSD[c.BlockData].BackTextureAllowed)
                                {
                                    int tf = rh.Cols.Count - vecsi.Count;
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        rh.Vertices.Add(new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z));
                                        int tx = tf + i;
                                        rh.Cols.Add(rh.Cols[tx]);
                                        rh.TCols.Add(rh.TCols[tx]);
                                        rh.Norms.Add(new Vector3(-normsi[i].X, -normsi[i].Y, -normsi[i].Z));
                                        rh.TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                        if (ths.Key != null)
                                        {
                                            rh.THVs.Add(new Vector4(ths.Key[i].X, ths.Key[i].Y, ths.Key[i].Z, ths.Key[i].W));
                                            rh.THWs.Add(new Vector4(ths.Value[i].X, ths.Value[i].Y, ths.Value[i].Z, ths.Value[i].W));
                                        }
                                        else
                                        {
                                            rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                            rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                for (int i = 0; i < rh.Vertices.Count; i += 3)
                {
                    Vector3 v1 = rh.Vertices[i];
                    Vector3 dv1 = rh.Vertices[i + 1] - v1;
                    Vector3 dv2 = rh.Vertices[i + 2] - v1;
                    Vector3 t1 = rh.TCoords[i];
                    Vector3 dt1 = rh.TCoords[i + 1] - t1;
                    Vector3 dt2 = rh.TCoords[i + 2] - t1;
                    Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                    Vector3 normal = rh.Norms[i];
                    tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized(); // TODO: Necessity of this correction?
                    rh.Tangs.Add(tangent);
                    rh.Tangs.Add(tangent);
                    rh.Tangs.Add(tangent);
                }
                if (rh.Vertices.Count == 0)
                {
                    OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                    {
                        OwningRegion.TheClient.ChunksRenderingCurrently--;
                        if (_VBO != null)
                        {
                            VBO tV = _VBO;
                            lock (OwningRegion.TheClient.vbos)
                            {
                                if (OwningRegion.TheClient.vbos.Count < 40)
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
                    OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                    {
                        IsAir = true;
                        _VBO = null;
                        if (DENIED)
                        {
                            return;
                        }
                        if (callback != null)
                        {
                            callback.Invoke();
                        }
                    });
                    return;
                }
                uint[] inds = new uint[rh.Vertices.Count];
                for (uint i = 0; i < rh.Vertices.Count; i++)
                {
                    inds[i] = i;
                }
                VBO tVBO;
                lock (OwningRegion.TheClient.vbos)
                {
                    if (OwningRegion.TheClient.vbos.Count > 0)
                    {
                        tVBO = OwningRegion.TheClient.vbos.Pop();
                    }
                    else
                    {
                        tVBO = new VBO();
                        tVBO.BufferMode = OpenTK.Graphics.OpenGL4.BufferUsageHint.StreamDraw;
                    }
                }
                lock (locky)
                {
                    tVBO.indices = inds;
                    tVBO.Vertices = rh.Vertices;
                    tVBO.Normals = rh.Norms;
                    tVBO.TexCoords = rh.TCoords;
                    tVBO.Colors = rh.Cols;
                    tVBO.TCOLs = rh.TCols;
                    tVBO.THVs = rh.THVs;
                    tVBO.THWs = rh.THWs;
                    tVBO.Tangents = rh.Tangs;
                    tVBO.BoneWeights = null;
                    tVBO.BoneIDs = null;
                    tVBO.BoneWeights2 = null;
                    tVBO.BoneIDs2 = null;
                    tVBO.oldvert();
                }
                OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                {
                    OwningRegion.TheClient.ChunksRenderingCurrently--;
                    if (DENIED)
                    {
                        if (tVBO.generated)
                        {
                            tVBO.Destroy();
                        }
                        return;
                    }
                    if (_VBO != null)
                    {
                        VBO tV = _VBO;
                        lock (OwningRegion.TheClient.vbos)
                        {
                            if (OwningRegion.TheClient.vbos.Count < 40)
                            {
                                OwningRegion.TheClient.vbos.Push(tV);
                            }
                            else
                            {
                                tV.Destroy();
                            }
                        }
                    }
                    if (DENIED)
                    {
                        if (tVBO.generated)
                        {
                            tVBO.Destroy();
                        }
                        return;
                    }
                    _VBO = tVBO;
                    lock (locky)
                    {
                        tVBO.GenerateOrUpdate();
                        tVBO.CleanLists();
                    }
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

        public bool IsAir = false;
        
        public void Render()
        {
            if (_VBO != null && _VBO.generated)
            {
                _VBO.Render(OwningRegion.TheClient.RenderTextures);
            }
        }
    }

    public class ChunkRenderHelper
    {
        const int CSize = Chunk.CHUNK_SIZE;

        public ChunkRenderHelper()
        {
            Vertices = new List<Vector3>(CSize * CSize * CSize * 6);
            TCoords = new List<Vector3>(CSize * CSize * CSize * 6);
            Norms = new List<Vector3>(CSize * CSize * CSize * 6);
            Cols = new List<Vector4>(CSize * CSize * CSize * 6);
            TCols = new List<Vector4>(CSize * CSize * CSize * 6);
            THVs = new List<Vector4>(CSize * CSize * CSize * 6);
            THWs = new List<Vector4>(CSize * CSize * CSize * 6);
            Tangs = new List<Vector3>(CSize * CSize * CSize * 6);
    }
        public List<Vector3> Vertices;
        public List<Vector3> TCoords;
        public List<Vector3> Norms;
        public List<Vector4> Cols;
        public List<Vector4> TCols;
        public List<Vector4> THVs;
        public List<Vector4> THWs;
        public List<Vector3> Tangs;
    }
}
