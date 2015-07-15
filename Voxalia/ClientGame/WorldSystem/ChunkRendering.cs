using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System.Threading;
using System.Threading.Tasks;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Chunk
    {
        public VBO _VBO = null;

        public void CreateVBO()
        {
            if (rendering != null && rendering.Status != TaskStatus.Canceled && rendering.Status != TaskStatus.RanToCompletion && rendering.Status != TaskStatus.Faulted && rendering.Status != TaskStatus.Created)
            {
                Task orend = rendering;
                rendering = new Task(() => VBOHInternal());
                orend.ContinueWith((o) => rendering.Start());
            }
            else
            {
                rendering = new Task(() => VBOHInternal());
                rendering.Start();
            }
        }

        public Task rendering = null;

        void VBOHInternal()
        {
            try
            {
                List<Vector3> Vertices = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6); // TODO: Make this an array?
                List<Vector3> TCoords = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6);
                List<Vector3> Norms = new List<Vector3>(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE * 6);
                Vector3 ppos = WorldPosition.ToOVector() * 30;
                for (int x = 0; x < CHUNK_SIZE; x++)
                {
                    for (int y = 0; y < CHUNK_SIZE; y++)
                    {
                        for (int z = 0; z < CHUNK_SIZE; z++)
                        {
                            BlockInternal c = GetBlockAt(x, y, z);
                            if (((Material)c.BlockMaterial).RendersAtAll())
                            {
                                BlockInternal zp = z + 1 < CHUNK_SIZE ? GetBlockAt(x, y, z + 1) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, y, 30));
                                BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, y, -1));
                                BlockInternal yp = y + 1 < CHUNK_SIZE ? GetBlockAt(x, y + 1, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, 30, z));
                                BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, -1, z));
                                BlockInternal xp = x + 1 < CHUNK_SIZE ? GetBlockAt(x + 1, y, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(30, y, z));
                                BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(-1, y, z));
                                bool zps = ((Material)zp.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[zp.BlockData].OccupiesTOP();
                                bool zms = ((Material)zm.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[zm.BlockData].OccupiesBOTTOM();
                                bool xps = ((Material)xp.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[xp.BlockData].OccupiesXP();
                                bool xms = ((Material)xm.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[xm.BlockData].OccupiesXM();
                                bool yps = ((Material)yp.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[yp.BlockData].OccupiesYP();
                                bool yms = ((Material)ym.BlockMaterial).IsOpaque() && BlockShapeRegistry.BSD[ym.BlockData].OccupiesYM();
                                Vector3 pos = new Vector3(ppos.X + x, ppos.Y + y, ppos.Z + z);
                                List<BEPUutilities.Vector3> vecsi = BlockShapeRegistry.BSD[c.BlockData].GetVertices(new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z), xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < vecsi.Count; i++)
                                {
                                    Vertices.Add(new Vector3(vecsi[i].X, vecsi[i].Y, vecsi[i].Z));
                                }
                                List<BEPUutilities.Vector3> normsi = BlockShapeRegistry.BSD[c.BlockData].GetNormals(new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z), xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < normsi.Count; i++)
                                {
                                    Norms.Add(new Vector3(normsi[i].X, normsi[i].Y, normsi[i].Z));
                                }
                                List<BEPUutilities.Vector3> tci = BlockShapeRegistry.BSD[c.BlockData].GetTCoords(new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z), (Material)c.BlockMaterial, xps, xms, yps, yms, zps, zms);
                                for (int i = 0; i < tci.Count; i++)
                                {
                                    TCoords.Add(new Vector3(tci[i].X, tci[i].Y, tci[i].Z));
                                }
                                if (vecsi.Count != normsi.Count || normsi.Count != tci.Count)
                                {
                                    SysConsole.Output(OutputType.INFO, "v:" + vecsi.Count + ",n:" + normsi.Count + ",tci:" + tci.Count);
                                }
                            }
                        }
                    }
                }
                if (Vertices.Count == 0)
                {
                    lock (OwningWorld.TheClient.TickLock)
                    {
                        if (_VBO != null)
                        {
                            VBO tV = _VBO;
                            OwningWorld.TheClient.Schedule.AddSyncTask(new Task(() => tV.Destroy()));
                        }
                        _VBO = null;
                    }
                    return;
                }
                List<uint> inds = new List<uint>(Vertices.Count); // TODO: VBO Array input instead of a list
                for (uint i = 0; i < Vertices.Count; i++)
                {
                    inds.Add(i);
                }
                VBO tVBO = new VBO();
                tVBO.Indices = inds;
                tVBO.Vertices = Vertices;
                tVBO.Normals = Norms;
                tVBO.TexCoords = TCoords;
                tVBO.BoneWeights = null;
                tVBO.BoneIDs = null;
                tVBO.BoneWeights2 = null;
                tVBO.BoneIDs2 = null;
                tVBO.Colors = null;
                lock (OwningWorld.TheClient.TickLock)
                {
                    if (_VBO != null)
                    {
                        VBO tV = _VBO;
                        OwningWorld.TheClient.Schedule.AddSyncTask(new Task(() => tV.Destroy()));
                    }
                    _VBO = tVBO;
                    OwningWorld.TheClient.Schedule.AddSyncTask(new Task(() => { tVBO.GenerateVBO(); tVBO.CleanLists(); }));
                }
            }
            catch (Exception ex)
            {
                if (ex is ThreadAbortException)
                {
                    throw ex;
                }
                SysConsole.Output(OutputType.ERROR, "Generating ChunkVBO...: " + ex.ToString());
            }
        }

        public void Render()
        {
            if (_VBO != null && _VBO.generated)
            {
                _VBO.Render(OwningWorld.TheClient.RenderTextures);
            }
        }
    }
}
