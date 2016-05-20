using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;
using FreneticScript;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class VBO
    {
        uint _VertexVBO;
        uint _IndexVBO;
        uint _NormalVBO;
        uint _TexCoordVBO;
        uint _ColorVBO;
        uint _BoneIDVBO;
        uint _BoneWeightVBO;
        uint _BoneID2VBO;
        uint _BoneWeight2VBO;
        uint _TCOLVBO;
        uint _TangentVBO;
        public uint _VAO;

        public Texture Tex;
        public Texture Tex_Specular;
        public Texture Tex_Reflectivity;
        public Texture Tex_Normal;

        public List<Vector3> Vertices;
        public List<uint> Indices;
        public List<Vector3> Normals;
        public List<Vector3> Tangents;
        public List<Vector3> TexCoords;
        public List<Vector4> Colors;
        public List<Vector4> BoneIDs;
        public List<Vector4> BoneWeights;
        public List<Vector4> BoneIDs2;
        public List<Vector4> BoneWeights2;
        public List<Vector4> TCOLs;

        public void CleanLists()
        {
            Vertices = null;
            Indices = null;
            Normals = null;
            Tangents = null;
            TexCoords = null;
            Colors = null;
            BoneIDs = null;
            BoneWeights = null;
            BoneIDs2 = null;
            BoneWeights2 = null;
            verts = null;
            indices = null;
            normals = null;
            texts = null;
            TCOLs = null;
        }

        int vC;

        public void AddSide(Location normal, TextureCoordinates tc, bool offs = false, float texf = 0)
        {
            // TODO: IMPROVE!
            for (int i = 0; i < 6; i++)
            {
                Normals.Add(ClientUtilities.Convert(normal));
                Colors.Add(new Vector4(1f, 1f, 1f, 1f));
                Indices.Add((uint)Indices.Count);
                BoneIDs.Add(new Vector4(0, 0, 0, 0));
                BoneWeights.Add(new Vector4(0f, 0f, 0f, 0f));
                BoneIDs2.Add(new Vector4(0, 0, 0, 0));
                BoneWeights2.Add(new Vector4(0f, 0f, 0f, 0f));
            }
            float aX = (tc.xflip ? 1 : 0) + tc.xshift;
            float aY = (tc.yflip ? 1 : 0) + tc.yshift;
            float bX = (tc.xflip ? 0 : 1) * tc.xscale + tc.xshift;
            float bY = (tc.yflip ? 1 : 0) + tc.yshift;
            float cX = (tc.xflip ? 0 : 1) * tc.xscale + tc.xshift;
            float cY = (tc.yflip ? 0 : 1) * tc.yscale + tc.yshift;
            float dX = (tc.xflip ? 1 : 0) + tc.xshift;
            float dY = (tc.yflip ? 0 : 1) * tc.yscale + tc.yshift;
            float zero = offs ? -0.5f: -1; // Sssh
            float one = offs ? 0.5f : 1;
            if (normal.Z == 1)
            {
                // T1
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, one));
                // T2
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, zero, one));
            }
            else if (normal.Z == -1)
            {
                // T1
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                // T2
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
            }
            else if (normal.X == 1)
            {
                // T1
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
                // T2
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
            }
            else if (normal.X == -1)
            {
                // T1
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                // T2
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
            }
            else if (normal.Y == 1)
            {
                // T1
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(zero, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
                // T2
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(one, one, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(one, one, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(zero, one, zero));
            }
            else if (normal.Y == -1)
            {
                // T1
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(aX, aY, texf));
                Vertices.Add(new Vector3(zero, zero, one));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                // T2
                TexCoords.Add(new Vector3(dX, dY, texf));
                Vertices.Add(new Vector3(zero, zero, zero));
                TexCoords.Add(new Vector3(bX, bY, texf));
                Vertices.Add(new Vector3(one, zero, one));
                TexCoords.Add(new Vector3(cX, cY, texf));
                Vertices.Add(new Vector3(one, zero, zero));
            }
            else
            {
                throw new Exception("Lazy code can't handle unique normals! Only axis-aligned, normalized normals!");
            }
        }

        public void Prepare()
        {
            Vertices = new List<Vector3>();
            Indices = new List<uint>();
            Normals = new List<Vector3>();
            TexCoords = new List<Vector3>();
            Colors = new List<Vector4>();
            BoneIDs = new List<Vector4>();
            BoneWeights = new List<Vector4>();
            BoneIDs2 = new List<Vector4>();
            BoneWeights2 = new List<Vector4>();
        }

        public bool generated = false;

        public void Destroy()
        {
            if (generated)
            {
                GL.DeleteVertexArray(_VAO);
                GL.DeleteBuffer(_VertexVBO);
                GL.DeleteBuffer(_IndexVBO);
                GL.DeleteBuffer(_NormalVBO);
                GL.DeleteBuffer(_TexCoordVBO);
                GL.DeleteBuffer(_TangentVBO);
                if (colors)
                {
                    GL.DeleteBuffer(_ColorVBO);
                    colors = false;
                }
                if (tcols)
                {
                    GL.DeleteBuffer(_TCOLVBO);
                    tcols = false;
                }
                if (bones)
                {
                    GL.DeleteBuffer(_BoneIDVBO);
                    GL.DeleteBuffer(_BoneWeightVBO);
                    GL.DeleteBuffer(_BoneID2VBO);
                    GL.DeleteBuffer(_BoneWeight2VBO);
                    bones = false;
                }
                generated = false;
            }
        }

        public void oldvert()
        {
            verts = Vertices.ToArray();
            normals = Normals.ToArray();
            texts = TexCoords.ToArray();
            Vertices = null;
            Normals = null;
            TexCoords = null;
            // TODO: Other arrays?
        }

        bool colors;
        bool tcols;
        bool bones;

        Vector3[] verts = null;
        public uint[] indices = null;
        Vector3[] normals = null;
        Vector3[] texts = null;

        public void UpdateBuffer()
        {
            if (Vertices == null && verts == null)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to render VBO, null vertices!");
                return;
            }
            Vector3[] vecs = verts == null ? Vertices.ToArray() : verts;
            uint[] inds = indices == null ? Indices.ToArray() : indices;
            Vector3[] norms = normals == null ? Normals.ToArray() : normals;
            Vector3[] texs = texts == null ? TexCoords.ToArray() : texts;
            Vector3[] tangs = Tangents != null ? Tangents.ToArray() : TangentsFor(vecs, norms, texs);
            Vector4[] cols = Colors != null ? Colors.ToArray() : null;
            Vector4[] tcols = TCOLs != null ? TCOLs.ToArray() : null;
            vC = vecs.Length;
            // Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes), vecs, BufferMode);
            // Normal buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes), norms, BufferMode);
            // TexCoord buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes), texs, BufferMode);
            // Tangent buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TangentVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangs.Length * Vector3.SizeInBytes), tangs, BufferMode);
            // Color buffer
            if (cols != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes), cols, BufferMode);
            }
            // TCOL buffer
            if (tcols != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _TCOLVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tcols.Length * Vector4.SizeInBytes), tcols, BufferMode);
            }
            // Index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(uint)), inds, BufferMode);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }

        public void GenerateOrUpdate()
        {
            if (generated)
            {
                UpdateBuffer();
            }
            else
            {
                GenerateVBO(false);
            }
        }

        public BufferUsageHint BufferMode = BufferUsageHint.StaticDraw;

        public Vector3[] TangentsFor(Vector3[] vecs, Vector3[] norms, Vector3[] texs)
        {
            Vector3[] tangs = new Vector3[vecs.Length];
            if (vecs.Length != norms.Length || texs.Length != vecs.Length || (vecs.Length % 3) != 0)
            {
                for (int i = 0; i < tangs.Length; i++)
                {
                    tangs[i] = new Vector3(0, 0, 0);
                }
                return tangs;
            }
            for (int i = 0; i < vecs.Length; i += 3)
            {
                Vector3 v1 = vecs[i];
                Vector3 dv1 = vecs[i + 1] - v1;
                Vector3 dv2 = vecs[i + 2] - v1;
                Vector3 t1 = texs[i];
                Vector3 dt1 = texs[i + 1] - t1;
                Vector3 dt2 = texs[i + 2] - t1;
                Vector3 tangent = (dv1 * dt2.Y - dv2 * dt1.Y) / (dt1.X * dt2.Y - dt1.Y * dt2.X);
                Vector3 normal = norms[i];
                tangent = (tangent - normal * Vector3.Dot(normal, tangent)).Normalized(); // TODO: Necessity of this correction?
                tangs[i] = tangent;
                tangs[i + 1] = tangent;
                tangs[i + 2] = tangent;
            }
            return tangs;
        }

        public void GenerateVBO(bool fixQuads = true, bool testFix = false)
        {
            if (generated)
            {
                Destroy();
            }
            GL.BindVertexArray(0);
            if (Vertices == null && verts == null)
            {
                SysConsole.Output(OutputType.ERROR, "Failed to render VBO, null vertices!");
                return;
            }
            Vector3[] vecs = verts == null ? Vertices.ToArray() : verts;
            if (vecs.Length == 0)
            {
                return;
            }
            uint[] inds = indices == null ? Indices.ToArray() : indices;
            Vector3[] norms = normals == null ? Normals.ToArray() : normals;
            Vector3[] texs = texts == null ? TexCoords.ToArray() : texts;
            Vector3[] tangs = Tangents != null ? Tangents.ToArray() : TangentsFor(vecs, norms, texs);
            Vector4[] cols = Colors != null ? Colors.ToArray() : (testFix ? new Vector4[vecs.Length] : null);
            Vector4[] tcols = TCOLs != null ? TCOLs.ToArray() : null;
            vC = inds.Length;
            Vector4[] ids = null;
            if (BoneIDs != null)
            {
                ids = BoneIDs.ToArray();
                bones = true;
            }
            else if (testFix)
            {
                ids = new Vector4[vecs.Length];
                bones = true;
            }
            Vector4[] weights = null;
            if (BoneWeights != null)
            {
                weights = BoneWeights.ToArray();
            }
            else if (testFix)
            {
                weights = new Vector4[vecs.Length];
            }
            Vector4[] ids2 = null;
            if (BoneIDs2 != null)
            {
                ids2 = BoneIDs2.ToArray();
            }
            else if (testFix)
            {
                ids2 = new Vector4[vecs.Length];
            }
            Vector4[] weights2 = null;
            if (BoneWeights2 != null)
            {
                weights2 = BoneWeights2.ToArray();
            }
            else if (testFix)
            {
                weights2 = new Vector4[vecs.Length];
            }
            if (fixQuads) // TODO: Remove need for this!
            {
                int vertc = (vecs.Length / 4) * 6;
                Vector3[] tvecs = new Vector3[vertc];
                Vector3[] tnorms = new Vector3[vertc];
                Vector3[] ttexs = new Vector3[vertc];
                Vector3[] ttangs = new Vector3[vertc];
                uint[] tinds = new uint[vertc];
                Vector4[] _cols = cols == null ? null : new Vector4[vertc];
                Vector4[] _tcols = tcols == null ? null : new Vector4[vertc];
                Vector4[] tids = ids == null ? null : new Vector4[vertc];
                Vector4[] tids2 = ids2 == null ? null : new Vector4[vertc];
                Vector4[] twei = weights == null ? null : new Vector4[vertc];
                Vector4[] twei2 = weights2 == null ? null : new Vector4[vertc];
                int x = 0;
                for (int i = 0; i < vecs.Length; i += 4)
                {
                    tvecs[x] = vecs[i];
                    tvecs[x + 1] = vecs[i + 1];
                    tvecs[x + 2] = vecs[i + 2];
                    tvecs[x + 3] = vecs[i];
                    tvecs[x + 4] = vecs[i + 2];
                    tvecs[x + 5] = vecs[i + 3];
                    tnorms[x] = norms[i];
                    tnorms[x + 1] = norms[i + 1];
                    tnorms[x + 2] = norms[i + 2];
                    tnorms[x + 3] = norms[i];
                    tnorms[x + 4] = norms[i + 2];
                    tnorms[x + 5] = norms[i + 3];
                    ttexs[x] = texs[i];
                    ttexs[x + 1] = texs[i + 1];
                    ttexs[x + 2] = texs[i + 2];
                    ttexs[x + 3] = texs[i];
                    ttexs[x + 4] = texs[i + 2];
                    ttexs[x + 5] = texs[i + 3];
                    ttangs[x] = texs[i];
                    ttangs[x + 1] = tangs[i + 1];
                    ttangs[x + 2] = tangs[i + 2];
                    ttangs[x + 3] = tangs[i];
                    ttangs[x + 4] = tangs[i + 2];
                    ttangs[x + 5] = tangs[i + 3];
                    for (int y = 0; y < 6; y++)
                    {
                        tinds[x + y] = (uint)(x + y);
                    }
                    if (_cols != null)
                    {
                        _cols[x] = cols[i];
                        _cols[x + 1] = cols[i + 1];
                        _cols[x + 2] = cols[i + 2];
                        _cols[x + 3] = cols[i];
                        _cols[x + 4] = cols[i + 2];
                        _cols[x + 5] = cols[i + 3];
                    }
                    if (_tcols != null)
                    {
                        _tcols[x] = tcols[i];
                        _tcols[x + 1] = tcols[i + 1];
                        _tcols[x + 2] = tcols[i + 2];
                        _tcols[x + 3] = tcols[i];
                        _tcols[x + 4] = tcols[i + 2];
                        _tcols[x + 5] = tcols[i + 3];
                    }
                    if (tids != null)
                    {
                        tids[x] = ids[i];
                        tids[x + 1] = ids[i + 1];
                        tids[x + 2] = ids[i + 2];
                        tids[x + 3] = ids[i];
                        tids[x + 4] = ids[i + 2];
                        tids[x + 5] = ids[i + 3];
                    }
                    if (tids2 != null)
                    {
                        tids2[x] = ids2[i];
                        tids2[x + 1] = ids2[i + 1];
                        tids2[x + 2] = ids2[i + 2];
                        tids2[x + 3] = ids2[i];
                        tids2[x + 4] = ids2[i + 2];
                        tids2[x + 5] = ids2[i + 3];
                    }
                    if (twei != null)
                    {
                        twei[x] = weights[i];
                        twei[x + 1] = weights[i + 1];
                        twei[x + 2] = weights[i + 2];
                        twei[x + 3] = weights[i];
                        twei[x + 4] = weights[i + 2];
                        twei[x + 5] = weights[i + 3];
                    }
                    if (twei2 != null)
                    {
                        twei2[x] = weights2[i];
                        twei2[x + 1] = weights2[i + 1];
                        twei2[x + 2] = weights2[i + 2];
                        twei2[x + 3] = weights2[i];
                        twei2[x + 4] = weights2[i + 2];
                        twei2[x + 5] = weights2[i + 3];
                    }
                    x += 6;
                }
                vecs = tvecs;
                norms = tnorms;
                texs = ttexs;
                ttangs = tangs;
                inds = tinds;
                cols = _cols;
                ids = tids;
                ids2 = tids2;
                weights = twei;
                weights2 = twei2;
            }
            // Vertex buffer
            GL.GenBuffers(1, out _VertexVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vecs.Length * Vector3.SizeInBytes), vecs, BufferMode);
            // Normal buffer
            GL.GenBuffers(1, out _NormalVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norms.Length * Vector3.SizeInBytes), norms, BufferMode);
            // TexCoord buffer
            GL.GenBuffers(1, out _TexCoordVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(texs.Length * Vector3.SizeInBytes), texs, BufferMode);
            GL.GenBuffers(1, out _TangentVBO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TangentVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tangs.Length * Vector3.SizeInBytes), tangs, BufferMode);
            // Color buffer
            if (cols != null)
            {
                colors = true;
                GL.GenBuffers(1, out _ColorVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cols.Length * Vector4.SizeInBytes), cols, BufferMode);
            }
            // TCOL buffer
            if (tcols != null)
            {
                this.tcols = true;
                GL.GenBuffers(1, out _TCOLVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _TCOLVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tcols.Length * Vector4.SizeInBytes), tcols, BufferMode);
            }
            // Weight buffer
            if (weights != null)
            {
                GL.GenBuffers(1, out _BoneWeightVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeightVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights.Length * Vector4.SizeInBytes), weights, BufferMode);
            }
            // ID buffer
            if (ids != null)
            {
                GL.GenBuffers(1, out _BoneIDVBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneIDVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids.Length * Vector4.SizeInBytes), ids, BufferMode);
            }
            // Weight2 buffer
            if (weights2 != null)
            {
                GL.GenBuffers(1, out _BoneWeight2VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeight2VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(weights2.Length * Vector4.SizeInBytes), weights2, BufferMode);
            }
            // ID2 buffer
            if (ids2 != null)
            {
                GL.GenBuffers(1, out _BoneID2VBO);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneID2VBO);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(ids2.Length * Vector4.SizeInBytes), ids2, BufferMode);
            }
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.GenBuffers(1, out _IndexVBO);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(inds.Length * sizeof(uint)), inds, BufferMode);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.GenVertexArrays(1, out _VAO);
            GL.BindVertexArray(_VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _VertexVBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _NormalVBO);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TexCoordVBO);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _TangentVBO);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
            if (cols != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _ColorVBO);
                GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (tcols != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _TCOLVBO);
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (weights != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeightVBO);
                GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (ids != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneIDVBO);
                GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (weights2 != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneWeight2VBO);
                GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            if (ids2 != null)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _BoneID2VBO);
                GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, 0, 0);
            }
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            if (cols != null)
            {
                GL.EnableVertexAttribArray(4);
            }
            if (tcols != null)
            {
                GL.EnableVertexAttribArray(5);
            }
            if (weights != null)
            {
                GL.EnableVertexAttribArray(5);
            }
            if (ids != null)
            {
                GL.EnableVertexAttribArray(6);
            }
            if (weights2 != null)
            {
                GL.EnableVertexAttribArray(7);
            }
            if (ids2 != null)
            {
                GL.EnableVertexAttribArray(8);
            }
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _IndexVBO);
            // Clean up
            GL.BindVertexArray(0);
            generated = true;

        }

        public static void BonesIdentity()
        {
            Matrix4 ident = Matrix4.Identity;
            int bones = 200;
            float[] floats = new float[bones * 4 * 4];
            for (int i = 0; i < bones; i++)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        floats[i * 16 + x * 4 + y] = ident[x, y];
                    }
                }
            }
            GL.UniformMatrix4(10, false, ref ident);
            GL.UniformMatrix4(11, bones, false, floats);
        }

        public void Render(bool texture, bool fixafter = true)
        {
            if (!generated)
            {
                return;
            }
            if (texture && Tex != null)
            {
                Tex.Bind();
                GL.ActiveTexture(TextureUnit.Texture1);
                if (Tex_Specular != null)
                {
                    Tex_Specular.Bind();
                }
                else
                {
                    Tex.Engine.Black.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture2);
                if (Tex_Reflectivity != null)
                {
                    Tex_Reflectivity.Bind();
                }
                else
                {
                    Tex.Engine.Black.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture3);
                if (Tex_Normal != null)
                {
                    Tex_Normal.Bind();
                }
                else
                {
                    Tex.Engine.NormalDef.Bind();
                }
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            GL.BindVertexArray(_VAO);
            GL.DrawElements(PrimitiveType.Triangles, vC, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.BindVertexArray(0);
            if (fixafter && texture && Tex != null)
            {
                Tex.Engine.White.Bind();
                GL.ActiveTexture(TextureUnit.Texture1);
                Tex.Engine.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                Tex.Engine.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture3);
                Tex.Engine.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }
    }

    public class TextureCoordinates
    {
        public TextureCoordinates()
        {
            xscale = 1;
            yscale = 1;
            xshift = 0;
            yshift = 0;
            xflip = false;
            yflip = false;
        }

        public float xscale;
        public float yscale;
        public float xshift;
        public float yshift;
        public bool xflip;
        public bool yflip;

        public override string ToString()
        {
            return xscale + "/" + yscale + "/" + xshift + "/" + yshift + "/" + (xflip ? "t" : "f") + "/" + (yflip ? "t" : "f");
        }

        public static TextureCoordinates FromString(string str)
        {
            TextureCoordinates tc = new TextureCoordinates();
            string[] data = str.SplitFast('/');
            tc.xscale = Utilities.StringToFloat(data[0]);
            tc.yscale = Utilities.StringToFloat(data[1]);
            tc.xshift = Utilities.StringToFloat(data[2]);
            tc.yshift = Utilities.StringToFloat(data[3]);
            tc.xflip = data[4] == "t";
            tc.yflip = data[5] == "t";
            return tc;
        }
    }
}
