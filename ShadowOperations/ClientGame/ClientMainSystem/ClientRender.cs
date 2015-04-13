using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double gDelta = 0;

        void InitRendering()
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            vpw = Window.Width;
            vph = Window.Height;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
        }

        public float CameraFOV = 45f;

        public Location CameraUp = Location.UnitZ;

        public float CameraZNear = 0.1f;
        public float CameraZFar = 10000f;

        void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            gDelta = e.Time;
            try
            {
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                GL.Enable(EnableCap.DepthTest);
                Location CameraPos = Player.GetPosition() + new Location(0, 0, Player.HalfSize.Z * 1.6f);
                Location CameraAngles = Player.GetAngles();
                double CameraYaw = CameraAngles.X;
                double CameraPitch = CameraAngles.Y;
                bool render_lighting = false;
                if (render_lighting)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    Shaders.ColorMultShader.Bind();
                    Location CameraTarget = CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
                    Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFOV), (float)Window.Width / (float)Window.Height, CameraZNear, CameraZFar);
                    Matrix4 view = Matrix4.LookAt(CameraPos.ToOVector(), CameraTarget.ToOVector(), CameraUp.ToOVector());
                    Matrix4 combined = view * proj;
                    GL.UniformMatrix4(1, false, ref combined);
                    Render3D();
                }
                GL.Disable(EnableCap.DepthTest);
                Shaders.ColorMultShader.Bind();
                Ortho = Matrix4.CreateOrthographicOffCenter(0, Window.Width, Window.Height, 0, -1, 1);
                Render2D();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Rendering: " + ex.ToString());
            }
            Window.SwapBuffers();
        }

        public void Render3D()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                Entities[i].Render();
            }
        }

        public void Render2D()
        {
            FontSets.Standard.DrawColoredText("^!^e^7gFPS(calc): " + (1f / gDelta) + "\n" + Player.GetPosition(), new Location(0, 0, 0));
            // TODO: Render the console
        }

        public int vpw = 800;
        public int vph = 600;

        public bool RenderLights = false;

        public Matrix4 Ortho;
    }
}
