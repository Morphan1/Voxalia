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

        BlockInternal TryAll(Location pos, int x, int y, int z)
        {
            if (PosMultiplier == 1)
            {
                return OwningRegion.GetBlockInternal(pos);
            }
            else
            {
                int px = x == 0 ? 1 : x * PosMultiplier;
                int py = y == 0 ? 1 : y * PosMultiplier;
                int pz = z == 0 ? 1 : z * PosMultiplier;
                for (int xe = 0; xe < px; xe++)
                {
                    for (int ye = 0; ye < py; ye++)
                    {
                        for (int ze = 0; ze < pz; ze++)
                        {
                            BlockInternal bi = OwningRegion.GetBlockInternal(pos + new Location(xe, ye, ze));
                            if (bi.BlockMaterial == 0 || bi.BlockData != 0)
                            {
                                return BlockInternal.AIR;
                            }
                        }
                    }
                }
                return new BlockInternal((ushort)Material.AIR, (byte)0, (byte)0); // NOTE: This also works if set to STONE or similar fullblocksolidopaque block.
            }
        }

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
                Vector3 ppos = ClientUtilities.Convert(WorldPosition * 30);
                bool light = OwningRegion.TheClient.CVars.r_fallbacklighting.ValueB;
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
                                BlockInternal zp = z + 1 < CSize ? GetBlockAt(x, y, z + 1) : TryAll(ClientUtilities.Convert(ppos) + new Location(x * PosMultiplier, y * PosMultiplier, 30), 1, 1, 0);
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : TryAll(ClientUtilities.Convert(ppos) + new Location(x * PosMultiplier, y * PosMultiplier, -1), 1, 1, 0);
                                BlockInternal yp = y + 1 < CSize ? GetBlockAt(x, y + 1, z) : TryAll(ClientUtilities.Convert(ppos) + new Location(x * PosMultiplier, 30, z * PosMultiplier), 1, 0, 1);
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : TryAll(ClientUtilities.Convert(ppos) + new Location(x * PosMultiplier, -1, z * PosMultiplier), 1, 0, 1);
                                BlockInternal xp = x + 1 < CSize ? GetBlockAt(x + 1, y, z) : TryAll(ClientUtilities.Convert(ppos) + new Location(30, y * PosMultiplier, z * PosMultiplier), 0, 1, 1);
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : TryAll(ClientUtilities.Convert(ppos) + new Location(-1, y * PosMultiplier, z * PosMultiplier), 0, 1, 1);
                                bool rAS = !((Material)c.BlockMaterial).GetCanRenderAgainstSelf();
                                bool zps = (((Material)zp.BlockMaterial).IsOpaque() || (rAS && (zp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zp.BlockData].OccupiesBOTTOM();
                                bool zms = (((Material)zm.BlockMaterial).IsOpaque() || (rAS && (zm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[zm.BlockData].OccupiesTOP();
                                bool xps = (((Material)xp.BlockMaterial).IsOpaque() || (rAS && (xp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXM();
                                bool xms = (((Material)xm.BlockMaterial).IsOpaque() || (rAS && (xm.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXP();
                                bool yps = (((Material)yp.BlockMaterial).IsOpaque() || (rAS && (yp.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYM();
                                bool yms = (((Material)ym.BlockMaterial).IsOpaque() || (rAS && (ym.BlockMaterial == c.BlockMaterial))) && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYP();
                                BEPUutilities.Vector3 pos = new BEPUutilities.Vector3(x, y, z);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(pos, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    // TODO: is PosMultiplier used correctly here?
                                    Vertices.Add(new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z));
                                }
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(pos, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < normsi.Count; i++)
                                {
                                    Norms.Add(new Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z));
                                }
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(pos, (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < tci.Count; i++)
                                {
                                    TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                }
                                if (vecsi.Count != normsi.Count || normsi.Count != tci.Count)
                                {
                                    SysConsole.Output(OutputType.WARNING, "PROBLEM RENDERING CHUNK: v:" + vecsi.Count + ",n:" + normsi.Count + ",tci:" + tci.Count);
                                }
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    float tp = c.BlockLocalData / 255f;
                                    float tc = 1;
                                    Vector3i me = new Vector3i(vecsi[i].X - x < 0.1 ? -1 : (vecsi[i].X - x > 0.9 ? 1 : 0),
                                        vecsi[i].Y - y < 0.1 ? -1 : (vecsi[i].Y - y > 0.9 ? 1 : 0),
                                        vecsi[i].Z - z < 0.1 ? -1 : (vecsi[i].Z - z > 0.9 ? 1 : 0));
                                    for (int f = 0; f < dirs.Length; f++)
                                    {
                                        Vector3i rel = dirs[f];
                                        if ((me.X == rel.X || rel.X == 0) && (me.Y == rel.Y || rel.Y == 0) && (me.Z == rel.Z || rel.Z == 0))
                                        {
                                            tp += SpecialGetBlockAt(x + rel.X, y + rel.Y, z + rel.Z).BlockLocalData / 255f;
                                            tc += 1;
                                        }
                                    }
                                    for (int f = 0; f < dirs.Length; f++)
                                    {
                                        Vector3i rel = dirs[f];
                                        rel.X = -rel.X;
                                        rel.Y = -rel.Y;
                                        rel.Z = -rel.Z;
                                        if ((me.X == rel.X || rel.X == 0) && (me.Y == rel.Y || rel.Y == 0) && (me.Z == rel.Z || rel.Z == 0))
                                        {
                                            tp += SpecialGetBlockAt(x + rel.X, y + rel.Y, z + rel.Z).BlockLocalData / 255f;
                                            tc += 1;
                                        }
                                    }
                                    float cCol = tp / tc;
                                    Cols.Add(new Vector4(cCol, cCol, cCol, 1f));
                                }
                                if (!((Material)c.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[c.BlockData].BackTextureAllowed)
                                {
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        Vertices.Add(new Vector3(vecsi[i].X * PosMultiplier + ppos.X, vecsi[i].Y * PosMultiplier + ppos.Y, vecsi[i].Z * PosMultiplier + ppos.Z));
                                    }
                                    for (int i = normsi.Count - 1; i >= 0; i--)
                                    {
                                        Norms.Add(new Vector3(-normsi[i].X, -normsi[i].Y, -normsi[i].Z));
                                    }
                                    for (int i = tci.Count - 1; i >= 0; i--)
                                    {
                                        TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                    }
                                    int txc = Cols.Count;
                                    for (int i = vecsi.Count - 1; i >= 0; i--)
                                    {
                                        Cols.Add(Cols[txc - i]);
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
                List<uint> inds = new List<uint>(Vertices.Count); // TODO: VBO Array input instead of a list?
                for (uint i = 0; i < Vertices.Count; i++)
                {
                    inds.Add(i);
                }
                if (Norms.Count != Vertices.Count)
                {
                    SysConsole.Output(OutputType.ERROR, "Normals invalid! Chunk at " + WorldPosition);
                }
                if (TCoords.Count != Vertices.Count)
                {
                    SysConsole.Output(OutputType.ERROR, "TexCoords invalid! Chunk at " + WorldPosition);
                }
                if (Cols.Count != Vertices.Count)
                {
                    SysConsole.Output(OutputType.ERROR, "Colors invalid! Chunk at " + WorldPosition + ", C: " + Cols.Count + ", V: " + Vertices.Count);
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
                tVBO.Indices = inds;
                tVBO.Vertices = Vertices;
                tVBO.Normals = Norms;
                tVBO.TexCoords = TCoords;
                tVBO.Colors = Cols;
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
