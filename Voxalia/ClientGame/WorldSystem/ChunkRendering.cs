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
            if (rendering != null && rendering.ThreadState == ThreadState.Running)
            {
                rendering.Abort();
                rendering = null;
            }
            rendering = new Thread(new ThreadStart(VBOHInternal));
            rendering.Start();
        }

        public Thread rendering = null;

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
                            BlockInternal zp = z + 1 < CHUNK_SIZE ? GetBlockAt(x, y, z + 1) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, y, 30));
                            BlockInternal zm = z > 0 ? GetBlockAt(x, y, z - 1) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, y, -1));
                            BlockInternal yp = y + 1 < CHUNK_SIZE ? GetBlockAt(x, y + 1, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, 30, z));
                            BlockInternal ym = y > 0 ? GetBlockAt(x, y - 1, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(x, -1, z));
                            BlockInternal xp = x + 1 < CHUNK_SIZE ? GetBlockAt(x + 1, y, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(30, y, z));
                            BlockInternal xm = x > 0 ? GetBlockAt(x - 1, y, z) : OwningWorld.GetBlockInternal(new Location(ppos) + new Location(-1, y, z));
                            if (((Material)c.BlockMaterial).IsOpaque() || ((Material)c.BlockMaterial).IsSolid()) // TODO: Better check. OccupiesFullBlock()?
                            {
                                Vector3 pos = new Vector3(ppos.X + x, ppos.Y + y, ppos.Z + z);
                                if (!((Material)zp.BlockMaterial).IsOpaque())
                                {
                                    int tID_TOP = ((Material)c.BlockMaterial).TextureID(MaterialSide.TOP);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(0, 0, 1));
                                    }
                                    TCoords.Add(new Vector3(0, 1, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(0, 0, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 0, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(0, 0, tID_TOP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                }
                                if (!((Material)zm.BlockMaterial).IsOpaque())
                                {
                                    int tID_BOTTOM = ((Material)c.BlockMaterial).TextureID(MaterialSide.BOTTOM);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(0, 0, -1));
                                    }
                                    TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(0, 1, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(0, 0, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(1, 0, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(1, 1, tID_BOTTOM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                                }
                                if (!((Material)xp.BlockMaterial).IsOpaque())
                                {
                                    int tID_XP = ((Material)c.BlockMaterial).TextureID(MaterialSide.XP);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(1, 0, 0));
                                    }
                                    TCoords.Add(new Vector3(0, 0, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(0, 1, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(0, 0, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 0, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_XP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                                }
                                if (!((Material)xm.BlockMaterial).IsOpaque())
                                {
                                    int tID_XM = ((Material)c.BlockMaterial).TextureID(MaterialSide.XM);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(-1, 0, 0));
                                    }
                                    TCoords.Add(new Vector3(0, 1, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(1, 1, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(0, 0, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(1, 0, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(0, 0, tID_XM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                }
                                if (!((Material)yp.BlockMaterial).IsOpaque())
                                {
                                    int tID_YP = ((Material)c.BlockMaterial).TextureID(MaterialSide.XP);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(0, 1, 0));
                                    }
                                    TCoords.Add(new Vector3(0, 1, tID_YP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(1, 1, tID_YP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(0, 0, tID_YP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_YP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z));
                                    TCoords.Add(new Vector3(1, 0, tID_YP));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y + 1, pos.Z + 1));
                                    TCoords.Add(new Vector3(0, 0, tID_YP));
                                    Vertices.Add(new Vector3(pos.X, pos.Y + 1, pos.Z + 1));
                                }
                                if (!((Material)ym.BlockMaterial).IsOpaque())
                                {
                                    int tID_YM = ((Material)c.BlockMaterial).TextureID(MaterialSide.YM);
                                    for (int i = 0; i < 6; i++)
                                    {
                                        Norms.Add(new Vector3(0, -1, 0));
                                    }
                                    TCoords.Add(new Vector3(0, 0, tID_YM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_YM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(0, 1, tID_YM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                    TCoords.Add(new Vector3(0, 0, tID_YM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 0, tID_YM));
                                    Vertices.Add(new Vector3(pos.X + 1, pos.Y, pos.Z + 1));
                                    TCoords.Add(new Vector3(1, 1, tID_YM));
                                    Vertices.Add(new Vector3(pos.X, pos.Y, pos.Z));
                                }
                                // TODO: Else, handle special case direction data
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
                List<Vector4> bWeight = new List<Vector4>(Vertices.Count);
                List<Vector4> bID = new List<Vector4>(Vertices.Count);
                List<Vector4> bWeight2 = new List<Vector4>(Vertices.Count);
                List<Vector4> bID2 = new List<Vector4>(Vertices.Count);
                List<Vector4> Colors = new List<Vector4>(Vertices.Count);
                for (uint i = 0; i < Vertices.Count; i++)
                {
                    inds.Add(i);
                    bWeight.Add(Vector4.Zero);
                    bID.Add(Vector4.Zero);
                    bWeight2.Add(Vector4.Zero);
                    bID2.Add(Vector4.Zero);
                    Colors.Add(Vector4.One);
                }
                VBO tVBO = new VBO();
                tVBO.Indices = inds;
                tVBO.Vertices = Vertices;
                tVBO.TexCoords = TCoords;
                tVBO.BoneWeights = bWeight;
                tVBO.BoneIDs = bID;
                tVBO.BoneWeights2 = bWeight2;
                tVBO.BoneIDs2 = bID2;
                tVBO.Colors = Colors;
                tVBO.Normals = Norms;
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
