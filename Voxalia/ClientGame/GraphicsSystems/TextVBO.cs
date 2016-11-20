//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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
        uint VBOTexCoords;
        uint VBOIndices;
        uint VBOColors;
        uint VAO;

        /// <summary>
        /// All vertices on this VBO.
        /// </summary>
        List<Vector3> Vecs = new List<Vector3>(100);
        
        List<Vector3> Texs = new List<Vector3>(100);
        List<Vector4> Cols = new List<Vector4>(100);

        //public void AddQuad(Vector2 min, Vector2 max, Vector2 tmin, Vector2 tmax, Vector4 color, int tex)
        public void AddQuad(float minX, float minY, float maxX, float maxY, float tminX, float tminY, float tmaxX, float tmaxY, Vector4 color, int tex)
        {
            Texs.Add(new Vector3(tminX, tminY, tex));
            Vecs.Add(new Vector3(minX, minY, 0));
            Cols.Add(color);
            Texs.Add(new Vector3(tmaxX, tminY, tex));
            Vecs.Add(new Vector3(maxX, minY, 0));
            Cols.Add(color);
            Texs.Add(new Vector3(tmaxX, tmaxY, tex));
            Vecs.Add(new Vector3(maxX, maxY, 0));
            Cols.Add(color);
            Texs.Add(new Vector3(tminX, tminY, tex));
            Vecs.Add(new Vector3(minX, minY, 0));
            Cols.Add(color);
            Texs.Add(new Vector3(tmaxX, tmaxY, tex));
            Vecs.Add(new Vector3(maxX, maxY, 0));
            Cols.Add(color);
            Texs.Add(new Vector3(tminX, tmaxY, tex));
            Vecs.Add(new Vector3(minX, maxY, 0));
            Cols.Add(color);
        }

        /// <summary>
        /// Destroys the internal VBO, so this can be safely deleted.
        /// </summary>
        public void Destroy()
        {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(VBOTexCoords);
            GL.DeleteBuffer(VBOColors);
            GL.DeleteBuffer(VBOIndices);
            GL.DeleteVertexArray(VAO);
            hasBuffers = false;
        }

        public void BuildBuffers()
        {
            GL.GenBuffers(1, out VBO);
            GL.GenBuffers(1, out VBOTexCoords);
            GL.GenBuffers(1, out VBOColors);
            GL.GenBuffers(1, out VBOIndices);
            GL.GenVertexArrays(1, out VAO);
            hasBuffers = true;
        }

        public int Length = 0;

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
            Vector3[] Positions = Vecs.ToArray();
            Vector3[] TexCoords = Texs.ToArray();
            Vector4[] Colors = Cols.ToArray();
            Length = Positions.Length;
            uint[] Indices = new uint[Length];
            for (uint i = 0; i < Length; i++)
            {
                Indices[i] = i;
            }
            Vecs.Clear();
            Texs.Clear();
            Cols.Clear();
            GL.BindVertexArray(0);
            // Vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Positions.Length * Vector3.SizeInBytes), Positions, BufferUsageHint.StaticDraw);
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
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOTexCoords);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 0, 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBOColors);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(0);
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
            if (Length == 0)
            {
                return;
            }
            GL.BindTexture(TextureTarget.Texture2DArray, Engine.Texture3D);
            Matrix4 mat = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(VAO);
            GL.DrawElements(PrimitiveType.Triangles, Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
