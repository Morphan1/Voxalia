using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;
using System.Linq;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems
{
    /// <summary>
    /// Rendering utility.
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Prepare the renderer.
        /// </summary>
        public void Init()
        {
            GenerateBoxVBO();
            GenerateSquareVBO();
            GenerateLineVBO();
            GenerateBackgridVBO();
        }

        public VBO Square;
        VBO Line;
        VBO Box;
        VBO Backgrid;

        void GenerateSquareVBO()
        {
            Vector3[] vecs = new Vector3[4];
            uint[] inds = new uint[4];
            Vector3[] norms = new Vector3[4];
            Vector3[] texs = new Vector3[4];
            Vector4[] cols = new Vector4[4];
            for (uint u = 0; u < 4; u++)
            {
                inds[u] = u;
            }
            for (int n = 0; n < 4; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 4; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[4];
            Vector4[] BoneWeights = new Vector4[4];
            Vector4[] BoneIDs2 = new Vector4[4];
            Vector4[] BoneWeights2 = new Vector4[4];
            for (int n = 0; n < 4; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(1, 0, 0);
            texs[0] = new Vector3(1, 0, 0);
            vecs[1] = new Vector3(1, 1, 0);
            texs[1] = new Vector3(1, 1, 0);
            vecs[2] = new Vector3(0, 1, 0);
            texs[2] = new Vector3(0, 1, 0);
            vecs[3] = new Vector3(0, 0, 0);
            texs[3] = new Vector3(0, 0, 0);
            Square = new VBO();
            Square.Vertices = vecs.ToList();
            Square.Indices = inds.ToList();
            Square.Normals = norms.ToList();
            Square.TexCoords = texs.ToList();
            Square.Colors = cols.ToList();
            Square.BoneIDs = BoneIDs.ToList();
            Square.BoneWeights = BoneWeights.ToList();
            Square.BoneIDs2 = BoneIDs2.ToList();
            Square.BoneWeights2 = BoneWeights2.ToList();
            Square.GenerateVBO();
        }

        void GenerateBackgridVBO()
        {
            Vector3[] vecs = new Vector3[4];
            uint[] inds = new uint[4];
            Vector3[] norms = new Vector3[4];
            Vector3[] texs = new Vector3[4];
            Vector4[] cols = new Vector4[4];
            for (uint u = 0; u < 4; u++)
            {
                inds[u] = u;
            }
            for (int n = 0; n < 4; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 4; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[4];
            Vector4[] BoneWeights = new Vector4[4];
            Vector4[] BoneIDs2 = new Vector4[4];
            Vector4[] BoneWeights2 = new Vector4[4];
            for (int n = 0; n < 4; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(10000, -10000, 0);
            texs[0] = new Vector3(1000, -1000, 0);
            vecs[1] = new Vector3(10000, 10000, 0);
            texs[1] = new Vector3(1000, 1000, 0);
            vecs[2] = new Vector3(-10000, 10000, 0);
            texs[2] = new Vector3(-1000, 1000, 0);
            vecs[3] = new Vector3(-10000, -10000, 0);
            texs[3] = new Vector3(-1000, -1000, 0);
            Backgrid = new VBO();
            Backgrid.Vertices = vecs.ToList();
            Backgrid.Indices = inds.ToList();
            Backgrid.Normals = norms.ToList();
            Backgrid.TexCoords = texs.ToList();
            Backgrid.Colors = cols.ToList();
            Backgrid.BoneIDs = BoneIDs.ToList();
            Backgrid.BoneWeights = BoneWeights.ToList();
            Backgrid.BoneIDs2 = BoneIDs2.ToList();
            Backgrid.BoneWeights2 = BoneWeights2.ToList();
            Backgrid.GenerateVBO();
        }

        void GenerateLineVBO()
        {
            Vector3[] vecs = new Vector3[2];
            uint[] inds = new uint[2];
            Vector3[] norms = new Vector3[2];
            Vector3[] texs = new Vector3[2];
            Vector4[] cols = new Vector4[2];
            for (uint u = 0; u < 2; u++)
            {
                inds[u] = u;
            }
            for (int n = 0; n < 2; n++)
            {
                norms[n] = new Vector3(0, 0, 1);
            }
            for (int c = 0; c < 2; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[2];
            Vector4[] BoneWeights = new Vector4[2];
            Vector4[] BoneIDs2 = new Vector4[2];
            Vector4[] BoneWeights2 = new Vector4[2];
            for (int n = 0; n < 2; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            vecs[0] = new Vector3(0, 0, 0);
            texs[0] = new Vector3(0, 0, 0);
            vecs[1] = new Vector3(1, 0, 0);
            texs[1] = new Vector3(1, 0, 0);
            Line = new VBO();
            Line.Vertices = vecs.ToList();
            Line.Indices = inds.ToList();
            Line.Normals = norms.ToList();
            Line.TexCoords = texs.ToList();
            Line.Colors = cols.ToList();
            Line.BoneIDs = BoneIDs.ToList();
            Line.BoneWeights = BoneWeights.ToList();
            Line.BoneIDs2 = BoneIDs2.ToList();
            Line.BoneWeights2 = BoneWeights2.ToList();
            Line.GenerateVBO();
        }

        void GenerateBoxVBO()
        {
            // TODO: Optimize?
            Vector3[] vecs = new Vector3[24];
            uint[] inds = new uint[24];
            Vector3[] norms = new Vector3[24];
            Vector3[] texs = new Vector3[24];
            Vector4[] cols = new Vector4[24];
            for (uint u = 0; u < 24; u++)
            {
                inds[u] = u;
            }
            for (int t = 0; t < 24; t++)
            {
                texs[t] = new Vector3(0, 0, 0);
            }
            for (int n = 0; n < 24; n++)
            {
                norms[n] = new Vector3(0, 0, 1); // TODO: Accurate normals somehow? Do lines even have normals?
            }
            for (int c = 0; c < 24; c++)
            {
                cols[c] = new Vector4(1, 1, 1, 1);
            }
            Vector4[] BoneIDs = new Vector4[24];
            Vector4[] BoneWeights = new Vector4[24];
            Vector4[] BoneIDs2 = new Vector4[24];
            Vector4[] BoneWeights2 = new Vector4[24];
            for (int n = 0; n < 24; n++)
            {
                BoneIDs[n] = new Vector4(0, 0, 0, 0);
                BoneWeights[n] = new Vector4(0, 0, 0, 0);
                BoneIDs2[n] = new Vector4(0, 0, 0, 0);
                BoneWeights2[n] = new Vector4(0, 0, 0, 0);
            }
            int i = 0;
            int zero = -1; // Ssh.
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(zero, zero, zero); i++;
            vecs[i] = new Vector3(zero, zero, 1); i++;
            vecs[i] = new Vector3(1, zero, zero); i++;
            vecs[i] = new Vector3(1, zero, 1); i++;
            vecs[i] = new Vector3(1, 1, zero); i++;
            vecs[i] = new Vector3(1, 1, 1); i++;
            vecs[i] = new Vector3(zero, 1, zero); i++;
            vecs[i] = new Vector3(zero, 1, 1); i++;
            Box = new VBO();
            Box.Vertices = vecs.ToList();
            Box.Indices = inds.ToList();
            Box.Normals = norms.ToList();
            Box.TexCoords = texs.ToList();
            Box.Colors = cols.ToList();
            Box.BoneIDs = BoneIDs.ToList();
            Box.BoneWeights = BoneWeights.ToList();
            Box.BoneIDs2 = BoneIDs2.ToList();
            Box.BoneWeights2 = BoneWeights2.ToList();
            Box.GenerateVBO();
        }

        public Renderer(TextureEngine tengine, ShaderEngine shaderdet)
        {
            Engine = tengine;
            Shaders = shaderdet;
        }

        public TextureEngine Engine;
        public ShaderEngine Shaders;

        /// <summary>
        /// Renders a black line box.
        /// </summary>
        public void RenderLineBox(Location min, Location max, Matrix4? rot = null)
        {
            Engine.White.Bind();
            Location halfsize = (max - min) / 2;
            Matrix4 mat = Matrix4.CreateScale(ClientUtilities.Convert(halfsize))
                * (rot != null && rot.HasValue ? rot.Value : Matrix4.Identity)
                * Matrix4.CreateTranslation(ClientUtilities.Convert(min + halfsize));
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Box._VAO);
            GL.DrawElements(PrimitiveType.Lines, 24, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        /// <summary>
        /// Render a line between two points.
        /// </summary>
        /// <param name="start">The initial point.</param>
        /// <param name="end">The ending point.</param>
        public void RenderLine(Location start, Location end)
        {
            // TODO: Efficiency!
            float len = (float)(end - start).Length();
            Location vecang = Utilities.VectorToAngles(start - end);
            Matrix4 mat = Matrix4.CreateScale(len, 1, 1)
                * Matrix4.CreateRotationY((float)(vecang.Y * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)(vecang.Z * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(start));
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Line._VAO);
            GL.DrawElements(PrimitiveType.Lines, 2, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        /// <summary>
        /// Render a cylinder between two points.
        /// </summary>
        /// <param name="start">The initial point.</param>
        /// <param name="end">The ending point.</param>
        public void RenderCylinder(Location start, Location end, float width)
        {
            float len = (float)(end - start).Length();
            Location vecang = Utilities.VectorToAngles(start - end);
            Matrix4 mat = Matrix4.CreateRotationY((float)(90 * Utilities.PI180))
                * Matrix4.CreateScale(len, width, width)
                * Matrix4.CreateRotationY((float)(vecang.Y * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)(vecang.Z * Utilities.PI180))
                 * Matrix4.CreateTranslation(ClientUtilities.Convert(start));
            GL.UniformMatrix4(2, false, ref mat);
            Client.Central.Models.Cylinder.Draw(); // TODO: Models reference in constructor - or client reference?
        }

        public void SetColor(Vector4 col)
        {
            if (!Client.Central.RenderingShadows)
            {
                GL.Uniform4(3, ref col);
            }
        }

        public void SetColor(Color4 c)
        {
            SetColor(new Vector4(c.R, c.G, c.B, c.A));
        }

        public void SetReflectionAmt(float val)
        {
            GL.Uniform1(8, val);
        }

        public void SetMinimumLight(float min)
        {
            if (Client.Central.RenderLights) // TODO: Pass client reference!
            {
                GL.Uniform1(5, min);
            }
        }

        public float Specular = 1;

        public void SetSpecular(float spec)
        {
            if (Client.Central.RenderLights && Client.Central.RenderSpecular) // TODO: Pass client reference!
            {
                Specular = spec;
                GL.Uniform1(6, spec);
            }
        }

        /// <summary>
        /// Renders a 2D rectangle.
        /// </summary>
        /// <param name="xmin">The lower bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymin">The lower bounds of the the rectangle: Y coordinate.</param>
        /// <param name="xmax">The upper bounds of the the rectangle: X coordinate.</param>
        /// <param name="ymax">The upper bounds of the the rectangle: Y coordinate.</param>
        public void RenderRectangle(float xmin, float ymin, float xmax, float ymax, Matrix4? rot = null)
        {
            Matrix4 mat = Matrix4.CreateScale(xmax - xmin, ymax - ymin, 1) * (rot != null && rot.HasValue ? rot.Value : Matrix4.Identity) * Matrix4.CreateTranslation(xmin, ymin, 0);
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        public void RenderBillboard(Location pos, Location scale, Location facing)
        {
            Location relang = Utilities.VectorToAngles(pos - facing);
            Matrix4 mat = Matrix4.CreateTranslation(-0.5f, -0.5f, 0f)
                * Matrix4.CreateScale(ClientUtilities.Convert(scale))
                * Matrix4.CreateRotationY((float)((relang.Y - 90) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)(relang.Z * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(pos));
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Square._VAO);
            GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        /// <summary>
        /// Renders a 2D backgrid.
        /// </summary>
        public void RenderBackgrid(Matrix4 mat)
        {
            GL.UniformMatrix4(2, false, ref mat);
            GL.BindVertexArray(Backgrid._VAO);
            GL.DrawElements(PrimitiveType.Quads, 4, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
