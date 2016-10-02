using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public VBO _VBO = null;

        public List<KeyValuePair<Vector3i, Material>> Lits = new List<KeyValuePair<Vector3i, Material>>();

        public bool Edited = true;

        public int Plant_VAO = -1;
        public int Plant_VBO_Pos = -1;
        public int Plant_VBO_Ind = -1;
        public int Plant_VBO_Col = -1;
        public int Plant_C;

        public void CreateVBO()
        {
            if (Edited)
            {
                Edited = false;
                lock (Lits)
                {
                    Lits.Clear();
                    if (CSize == CHUNK_SIZE)
                    {
                        for (int x = 0; x < CHUNK_SIZE; x++)
                        {
                            for (int y = 0; y < CHUNK_SIZE; y++)
                            {
                                for (int z = 0; z < CHUNK_SIZE; z++)
                                {
                                    BlockInternal bi = GetBlockAt(x, y, z);
                                    if (bi.Material.GetLightEmitRange() > 0)
                                    {
                                        Lits.Add(new KeyValuePair<Vector3i, Material>(new Vector3i(x, y, z), bi.Material));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            OwningRegion.NeedToRender(this);
        }

        /// <summary>
        /// Internal region call only.
        /// </summary>
        public void MakeVBONow()
        {
            if (rendering != null)
            {
                ASyncScheduleItem item = OwningRegion.TheClient.Schedule.AddASyncTask(() => VBOHInternal());
                rendering = rendering.ReplaceOrFollowWith(item);
            }
            else
            {
                rendering = OwningRegion.TheClient.Schedule.StartASyncTask(() => VBOHInternal());
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
            if (c.PosMultiplier > PosMultiplier)
            {
                return new BlockInternal((ushort)Material.STONE, 0, 0, 0);
            }
            return BlockInternal.AIR;
        }
        
        void VBOHInternal()
        {
            try
            {
                bool shaped = OwningRegion.TheClient.CVars.r_noblockshapes.ValueB;
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
                //bool light = OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB;
                Chunk c_zp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, 1));
                Chunk c_zm = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 0, -1));
                Chunk c_yp = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, 1, 0));
                Chunk c_ym = OwningRegion.GetChunk(WorldPosition + new Vector3i(0, -1, 0));
                Chunk c_xp = OwningRegion.GetChunk(WorldPosition + new Vector3i(1, 0, 0));
                Chunk c_xm = OwningRegion.GetChunk(WorldPosition + new Vector3i(-1, 0, 0));
                List<Chunk> potentials = new List<Chunk>() { this, c_zp, c_zm, c_yp, c_ym, c_xp, c_xm };
                BlockInternal t_air = new BlockInternal((ushort)Material.STONE, 0, 0, 0);
                List<Vector3> poses = new List<Vector3>();
                List<Vector4> colorses = new List<Vector4>();
                for (int x = 0; x < CSize; x++)
                {
                    for (int y = 0; y < CSize; y++)
                    {
                        for (int z = 0; z < CSize; z++)
                        {
                            BlockInternal c = GetBlockAt(x, y, z);
                            if ((c.Material).RendersAtAll())
                            {
                                BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : (c_zp == null ? t_air : GetMostSolid(c_zp, x, y, z + 1 - CSize));
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : (c_zm == null ? t_air : GetMostSolid(c_zm, x, y, z - 1 + CSize));
                                BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : (c_yp == null ? t_air : GetMostSolid(c_yp, x, y + 1 - CSize, z));
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : (c_ym == null ? t_air : GetMostSolid(c_ym, x, y - 1 + CSize, z));
                                BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : (c_xp == null ? t_air : GetMostSolid(c_xp, x + 1 - CSize, y, z));
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : (c_xm == null ? t_air : GetMostSolid(c_xm, x - 1 + CSize, y, z));
                                bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                                bool pMatters = !c.IsOpaque();
                                bool zps = (zp.IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial && (pMatters || zp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : zp.BlockData].OccupiesBOTTOM();
                                bool zms = (zm.IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial && (pMatters || zm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : zm.BlockData].OccupiesTOP();
                                bool xps = (xp.IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial && (pMatters || xp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : xp.BlockData].OccupiesXM();
                                bool xms = (xm.IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial && (pMatters || xm.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : xm.BlockData].OccupiesXP();
                                bool yps = (yp.IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial && (pMatters || yp.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : yp.BlockData].OccupiesYM();
                                bool yms = (ym.IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial && (pMatters || ym.BlockPaint == c.BlockPaint)))) && BlockShapeRegistry.BSD[shaped ? 0 : ym.BlockData].OccupiesYP();
                                if (zps && zms && xps && xms && yps && yms)
                                {
                                    continue;
                                }
                                int index_bssd = (xps ? 1 : 0) | (xms ? 2 : 0) | (yps ? 4 : 0) | (yms ? 8 : 0) | (zps ? 16 : 0) | (zms ? 32 : 0);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].BSSD.Verts[index_bssd];
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].BSSD.Norms[index_bssd];
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetTCoordsQuick(index_bssd, c.Material);
                                KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>> ths = !c.BlockShareTex ? default(KeyValuePair<List<BEPUutilities.Vector4>, List<BEPUutilities.Vector4>>) :
                                    BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].GetStretchData(new BEPUutilities.Vector3(x, y, z), vecsi, xp, xm, yp, ym, zp, zm, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    Vector3 vt = new Vector3((float)(x + vecsi[i].X) * PosMultiplier, (float)(y + vecsi[i].Y) * PosMultiplier, (float)(z + vecsi[i].Z) * PosMultiplier);
                                    rh.Vertices.Add(vt);
                                    Vector3 nt = new Vector3((float)normsi[i].X, (float)normsi[i].Y, (float)normsi[i].Z);
                                    rh.Norms.Add(nt);
                                    rh.TCoords.Add(new Vector3((float)tci[i].X, (float)tci[i].Y, (float)tci[i].Z));
                                    Location lcol = OwningRegion.GetLightAmount(ClientUtilities.Convert(vt) + WorldPosition.ToLocation() * CHUNK_SIZE, ClientUtilities.Convert(nt), potentials);
                                    rh.Cols.Add(new Vector4((float)lcol.X, (float)lcol.Y, (float)lcol.Z, 1));
                                    rh.TCols.Add(OwningRegion.TheClient.Rendering.AdaptColor(vt, Colors.ForByte(c.BlockPaint)));
                                    if (ths.Key != null)
                                    {
                                        rh.THVs.Add(new Vector4((float)ths.Key[i].X, (float)ths.Key[i].Y, (float)ths.Key[i].Z, (float)ths.Key[i].W));
                                        rh.THWs.Add(new Vector4((float)ths.Value[i].X, (float)ths.Value[i].Y, (float)ths.Value[i].Z, (float)ths.Value[i].W));
                                    }
                                    else
                                    {
                                        rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                        rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                    }
                                }
                                if (!c.IsOpaque() && BlockShapeRegistry.BSD[shaped ? 0 : c.BlockData].BackTextureAllowed)
                                {
                                    int tf = rh.Cols.Count - vecsi.Count;
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        Vector3 vt = new Vector3((float)(x + vecsi[i].X) * PosMultiplier, (float)(y + vecsi[i].Y) * PosMultiplier, (float)(z + vecsi[i].Z) * PosMultiplier);
                                        int tx = tf + i;
                                        rh.Cols.Add(rh.Cols[tx]);
                                        rh.TCols.Add(rh.TCols[tx]);
                                        rh.Norms.Add(new Vector3((float)-normsi[i].X, (float)-normsi[i].Y, (float)-normsi[i].Z));
                                        rh.TCoords.Add(new Vector3((float)tci[i].X, (float)tci[i].Y, (float)tci[i].Z));
                                        if (ths.Key != null)
                                        {
                                            rh.THVs.Add(new Vector4((float)ths.Key[i].X, (float)ths.Key[i].Y, (float)ths.Key[i].Z, (float)ths.Key[i].W));
                                            rh.THWs.Add(new Vector4((float)ths.Value[i].X, (float)ths.Value[i].Y, (float)ths.Value[i].Z, (float)ths.Value[i].W));
                                        }
                                        else
                                        {
                                            rh.THVs.Add(new Vector4(0, 0, 0, 0));
                                            rh.THWs.Add(new Vector4(0, 0, 0, 0));
                                        }
                                    }
                                }
                                if (PosMultiplier == 1 && c.Material.GetPlant() != null && !zp.Material.RendersAtAll() && zp.Material.GetSolidity() == MaterialSolidity.NONSOLID)
                                {
                                    if (BlockShapeRegistry.BSD[c.BlockData].Coll == null)
                                    {
                                        // TODO: BSD-level precompute this?
                                        Location offset;
                                        BEPUphysics.CollisionShapes.EntityShape es = BlockShapeRegistry.BSD[c.BlockData].GetShape(c.Damage, out offset, false);
                                        BlockShapeRegistry.BSD[c.BlockData].Coll = es.GetCollidableInstance();
                                        BlockShapeRegistry.BSD[c.BlockData].Coll.LocalPosition = -offset.ToBVector();
                                    }
                                    Location skylight = OwningRegion.GetLightAmount(new Location(WorldPosition.X * Chunk.CHUNK_SIZE + x + 0.5, WorldPosition.Y * Chunk.CHUNK_SIZE + y + 0.5,
                                        WorldPosition.Z * Chunk.CHUNK_SIZE + z + 1.0), Location.UnitZ, potentials);
                                    for (int plx = 0; plx < 3; plx++)
                                    {
                                        for (int ply = 0; ply < 3; ply++)
                                        {
                                            BEPUutilities.RayHit rayhit;
                                            if (!BlockShapeRegistry.BSD[c.BlockData].Coll.RayCast(new BEPUutilities.Ray(new BEPUutilities.Vector3(0.3333f * plx, 0.3333f * ply, 3), new BEPUutilities.Vector3(0, 0, -1)), 5, out rayhit))
                                            {
                                                rayhit.Location = new BEPUutilities.Vector3(0.3333 * plx, 0.3333 * ply, 1.0);
                                            }
                                            poses.Add(new Vector3(x + (float)rayhit.Location.X, y + (float)rayhit.Location.Y, z + (float)rayhit.Location.Z));
                                            colorses.Add(new Vector4((float)skylight.X, (float)skylight.Y, (float)skylight.Z, 1.0f));
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
                        IsAir = true;
                        _VBO = null;
                    });
                    OwningRegion.DoneRendering(this);
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
                Vector3[] posset = poses.ToArray();
                Vector4[] colorset = colorses.ToArray();
                uint[] posind = new uint[posset.Length];
                for (uint i = 0; i < posind.Length; i++)
                {
                    posind[i] = i;
                }
                OwningRegion.TheClient.Schedule.ScheduleSyncTask(() =>
                {
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
                    DestroyPlants();
                    Plant_VAO = GL.GenVertexArray();
                    Plant_VBO_Ind = GL.GenBuffer();
                    Plant_VBO_Pos = GL.GenBuffer();
                    Plant_VBO_Col = GL.GenBuffer();
                    Plant_C = posind.Length;
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Plant_VBO_Pos);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(posset.Length * OpenTK.Vector3.SizeInBytes), posset, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Plant_VBO_Col);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(colorset.Length * OpenTK.Vector4.SizeInBytes), colorset, BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, Plant_VBO_Ind);
                    GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(posind.Length * sizeof(uint)), posind, BufferUsageHint.StaticDraw);
                    GL.BindVertexArray(Plant_VAO);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Plant_VBO_Pos);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Plant_VBO_Col);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(4);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, Plant_VBO_Ind);
                });
                OwningRegion.DoneRendering(this);
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Generating ChunkVBO...: " + ex.ToString());
                OwningRegion.DoneRendering(this);
            }
        }

        public bool IsAir = false;
        
        public void Render()
        {
            if (_VBO != null && _VBO.generated)
            {
                Matrix4d mat = Matrix4d.CreateTranslation(ClientUtilities.ConvertD(WorldPosition.ToLocation() * CHUNK_SIZE));
                OwningRegion.TheClient.MainWorldView.SetMatrix(2, mat);
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
