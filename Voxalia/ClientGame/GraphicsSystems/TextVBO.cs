using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class TextVBO
    {
        public GLFontEngine Engine;

        public TextVBO(GLFontEngine fengine)
        {
            Engine = fengine;
        }

        uint VBO;
        uint VBONormals;
        uint VBOTexCoords;
        uint VBOIndices;
        uint VBOColors;
        uint VAO;
        Vector3[] Positions;
        Vector3[] Normals;
        Vector3[] TexCoords;
        Vector4[] Colors;
        uint[] Indices;

        /// <summary>
        /// All vertices on this VBO.
        /// </summary>
        List<Vector3> Vecs = new List<Vector3>(100);

        List<Vector3> Norms = new List<Vector3>(100);
        List<Vector3> Texs = new List<Vector3>(100);
        List<uint> Inds = new List<uint>();
        List<Vector4> Cols = new List<Vector4>(100);

        public void AddQuad(Vector2 min, Vector2 max, Vector2 tmin, Vector2 tmax, Vector4 color, int tex)
        {
            Norms.Add(new Vector3(0, 0, 1));
            Texs.Add(new Vector3(tmin.X, tmin.Y, tex));
            Vecs.Add(new Vector3(min.X, min.Y, 0));
            Inds.Add((uint)(Vecs.Count - 1));
            Cols.Add(color);
            Norms.Add(new Vector3(0, 0, 1));
            Texs.Add(new Vector3(tmax.X, tmin.Y, tex));
            Vecs.Add(new Vector3(max.X, min.Y, 0));
            Inds.Add((uint)(Vecs.Count - 1));
            Cols.Add(color);
            Norms.Add(new Vector3(0, 0, 1));
            Texs.Add(new Vector3(tmax.X, tmax.Y, tex));
            Vecs.Add(new Vector3(max.X, max.Y, 0));
            Inds.Add((uint)(Vecs.Count - 1));
            Cols.Add(color);
            Norms.Add(new Vector3(0, 0, 1));
            Texs.Add(new Vector3(tmin.X, tmax.Y, tex));
            Vecs.Add(new Vector3(min.X, max.Y, 0));
            Inds.Add((uint)(Vecs.Count - 1));
            Cols.Add(color);
        }

        /// <summary>
        /// Destroys the internal VBO, so this can be safely deleted.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(VBONormals);
            GL.DeleteBuffer(VBOTexCoords);
            GL.DeleteBuffer(VBOColors);
            GL.DeleteBuffer(VBOIndices);
            GL.DeleteVertexArray(VAO);
            hasBuffers = false;
        }

        public void BuildBuffers()
        {
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out VBONormals);
            GL.GenBuffers(1, out VBOTexCoords);
            GL.GenBuffers(1, out VBOColors);
            GL.GenBuffers(1, out VBOIndices);
            GL.GenVertexArrays(1, out VAO);
            hasBuffers = true;
        }

        bool hasBuffers = false;

        /// <summary>
        /// Turns the local VBO build information into an actual internal GPU-side VBO.
        /// </summary>
        public void Build()
        {
            if (!hasBuffers)
            {
                BuildBuffers();
            }
            Positions = Vecs.ToArray();
            Normals = Norms.ToArray();
            TexCoords = Texs.ToArray();
            Indices = Inds.ToArray();
            Colors = Cols.ToArray();
            Vecs.Clear();
            Norms.Clear();
            Texs.Clear();
            Inds.Clear();
            Cols.Clear();
            GL.BindVertexArray(0);
            // Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Positions.Length * Vector3.SizeInBytes), Positions, BufferUsageHint.StaticDraw);
            // Normal buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormals);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Normals.Length * Vector3.SizeInBytes), Normals, BufferUsageHint.StaticDraw);
            // TexCoord buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TexCoords.Length * Vector3.SizeInBytes), TexCoords, BufferUsageHint.StaticDraw);
            // Color buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Colors.Length * Vector4.SizeInBytes), Colors, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            // Index buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndices);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(Indices.Length * sizeof(uint)), Indices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            // VAO
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBONormals);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, VBOIndices);
            // Clean up
            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Renders the internal VBO to screen.
        /// </summary>
        public void Render()
        {
            if (Positions.Length == 0 || Indices.Length == 0)
            {
                return;
            }
            GL.BindTexture(TextureTarget.Texture2DArray, Engine.Texture3D);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Quads, Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
