using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.WorldSystem
{
    public partial class Region
    {
        public Location Wind = new Location(0.3, 0, 0); // TODO: Gather this value from the server!

        public Location ActualWind = new Location(0.8, 0, 0);

        public void TickClouds()
        {
            ActualWind = Wind * Math.Sin(GlobalTickTimeLocal * 0.6);
            for (int i = 0; i < Clouds.Count; i++)
            {
                Clouds[i].Position += Clouds[i].Velocity * Delta;
                for (int s = 0; s < Clouds[i].Sizes.Count; s++)
                {
                    Clouds[i].Sizes[s] += 0.05f * (float)Delta;
                    if (Clouds[i].Sizes[s] > Clouds[i].EndSizes[s])
                    {
                        Clouds[i].Sizes[s] = Clouds[i].EndSizes[s];
                    }
                }
            }
            Cl_c += Delta;
            if (Cl_c > 0.5)
            {
                ReClouds = true;
                Cl_c = 0;
            }
        }

        double Cl_c = 0;

        public List<Cloud> Clouds = new List<Cloud>();

        List<Vector3> Cl_Pos = new List<Vector3>();
        List<Vector3> Cl_Norm = new List<Vector3>();
        List<Vector2> Cl_TC = new List<Vector2>();
        List<Vector4> Cl_Col = new List<Vector4>();
        List<uint> Cl_Ind = new List<uint>();

        int Cl_VAO = -1;
        int Cl_VBO_Pos = -1;
        int Cl_VBO_Norm = -1;
        int Cl_VBO_TC = -1;
        int Cl_VBO_Tang = -1;
        int Cl_VBO_Col = -1;
        int Cl_VBO_Ind = -1;
        int Cl_VBO_BID1 = -1;
        int Cl_VBO_BID2 = -1;
        int Cl_VBO_BWE1 = -1;
        int Cl_VBO_BWE2 = -1;

        static Vector3[] Cl_Vecer = new Vector3[] { new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
                new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 0) };

        static Vector2[] Cl_TCer = new Vector2[] { new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(1, 0), new Vector2(0, 1), new Vector2(0, 0) };

        public bool ReClouds = true;

        public void RenderClouds()
        {
            if (!TheClient.CVars.r_clouds.ValueB)
            {
                if (Cl_Pos.Count > 0)
                {
                    Cl_Pos = new List<Vector3>();
                    Cl_Norm = new List<Vector3>();
                    Cl_TC = new List<Vector2>();
                    Cl_Ind = new List<uint>();
                }
                return;
            }
            if (ReClouds)
            {
                bool forceset = false;
                if (Cl_VAO == -1)
                {
                    Cl_VAO = GL.GenVertexArray();
                    Cl_VBO_Pos = GL.GenBuffer();
                    Cl_VBO_Norm = GL.GenBuffer();
                    Cl_VBO_TC = GL.GenBuffer();
                    Cl_VBO_Tang = GL.GenBuffer();
                    Cl_VBO_Ind = GL.GenBuffer();
                    Cl_VBO_Col = GL.GenBuffer();
                    Cl_VBO_BID1 = GL.GenBuffer();
                    Cl_VBO_BID2 = GL.GenBuffer();
                    Cl_VBO_BWE1 = GL.GenBuffer();
                    Cl_VBO_BWE2 = GL.GenBuffer();
                    forceset = true;
                }
                Cl_Pos.Clear();
                Cl_Norm.Clear();
                Cl_TC.Clear();
                Cl_Ind.Clear();
                foreach (Cloud cloud in Clouds)
                {
                    Location relang = Utilities.VectorToAngles(cloud.Position - TheClient.MainWorldView.CameraPos);
                    Matrix4 mat = Matrix4.CreateRotationY((float)((relang.Y - 90) * Utilities.PI180))
                        * Matrix4.CreateRotationZ((float)(relang.Z * Utilities.PI180))
                        * Matrix4.CreateTranslation(ClientUtilities.Convert(cloud.Position));
                    for (int i = 0; i < cloud.Points.Count; i++)
                    {
                        Matrix4 pmat = Matrix4.CreateTranslation(-0.5f, -0.5f, 0f) * Matrix4.CreateScale(cloud.Sizes[i]) * mat * Matrix4.CreateTranslation(ClientUtilities.Convert(cloud.Points[i]));
                        Matrix4 nmat = pmat;
                        nmat[3, 0] = 0;
                        nmat[3, 1] = 0;
                        nmat[3, 2] = 0;
                        for (uint n = 0; n < 6; n++)
                        {
                            Cl_Ind.Add((uint)Cl_Ind.Count);
                            Cl_Col.Add(Vector4.One);
                            Cl_Norm.Add(Vector4.Transform(nmat, new Vector4(0, 0, 1, 1)).Xyz);
                            Vector4 p = Vector4.Transform(new Vector4(Cl_Vecer[n], 1f), pmat);
                            Cl_Pos.Add(p.Xyz / p.W);
                            Cl_TC.Add(Cl_TCer[n]);
                        }
                    }
                }
                Vector3[] pos = Cl_Pos.ToArray();
                Vector3[] norm = Cl_Norm.ToArray();
                Vector2[] tc = Cl_TC.ToArray();
                Vector3[] tang = VBO.TangentsFor(pos, norm, tc);
                Vector4[] col = Cl_Col.ToArray();
                uint[] ind = Cl_Ind.ToArray();
                Vector4[] bdata = new Vector4[pos.Length];
                for (int i = 0; i < bdata.Length; i++)
                {
                    bdata[i] = new Vector4(0f, 0f, 0f, 0f);
                }
                BufferUsageHint BufferMode = BufferUsageHint.StreamDraw;
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Pos);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(pos.Length * Vector3.SizeInBytes), pos, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Norm);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(norm.Length * Vector3.SizeInBytes), norm, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_TC);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tc.Length * Vector2.SizeInBytes), tc, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Tang);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(tang.Length * Vector3.SizeInBytes), tang, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Col);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(col.Length * Vector4.SizeInBytes), col, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BWE1);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(bdata.Length * Vector4.SizeInBytes), bdata, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BID1);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(bdata.Length * Vector4.SizeInBytes), bdata, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BWE2);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(bdata.Length * Vector4.SizeInBytes), bdata, BufferMode);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BID2);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(bdata.Length * Vector4.SizeInBytes), bdata, BufferMode);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Cl_VBO_Ind);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(ind.Length * sizeof(uint)), ind, BufferMode);
                if (forceset)
                {
                    GL.BindVertexArray(Cl_VAO);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Pos);
                    GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Norm);
                    GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_TC);
                    GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Tang);
                    GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_Col);
                    GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BWE1);
                    GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BID1);
                    GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BWE2);
                    GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, Cl_VBO_BID2);
                    GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, 0, 0);
                    GL.EnableVertexAttribArray(0);
                    GL.EnableVertexAttribArray(1);
                    GL.EnableVertexAttribArray(2);
                    GL.EnableVertexAttribArray(3);
                    GL.EnableVertexAttribArray(4);
                    GL.EnableVertexAttribArray(5);
                    GL.EnableVertexAttribArray(6);
                    GL.EnableVertexAttribArray(7);
                    GL.EnableVertexAttribArray(8);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, Cl_VBO_Ind);
                }
                ReClouds = false;
            }
            TheClient.SetEnts();
            TheClient.Textures.GetTexture("effects/clouds/cloud1").Bind(); // TODO: Cache!
            TheClient.MainWorldView.SetMatrix(2, Matrix4d.Identity);
            Matrix4 identity = Matrix4.Identity;
            GL.UniformMatrix4(40, false, ref identity);
            GL.BindVertexArray(Cl_VAO);
            GL.DrawElements(PrimitiveType.Triangles, Cl_Ind.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }
    }
}
