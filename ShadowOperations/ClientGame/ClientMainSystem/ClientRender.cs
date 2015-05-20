using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.UISystem;
using ShadowOperations.ClientGame.GraphicsSystems;
using ShadowOperations.ClientGame.GraphicsSystems.LightingSystem;
using ShadowOperations.ClientGame.OtherSystems;
using ShadowOperations.ClientGame.JointSystem;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double gDelta = 0;

        public List<LightObject> Lights = new List<LightObject>();

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
            s_shadow = Shaders.GetShader("shadow");
            s_main = Shaders.GetShader("test");
            s_fbo = Shaders.GetShader("fbo");
            s_shadowadder = Shaders.GetShader("shadowadder");
            s_transponly = Shaders.GetShader("transponly");
            generateLightHelpers();
            ambient = new Location(0.02f);
        }

        public void destroyLightHelpers()
        {
            RS4P.Destroy();
            GL.DeleteFramebuffer(fbo_main);
            GL.DeleteFramebuffer(fbo2_main);
            GL.DeleteTexture(fbo_texture);
            GL.DeleteTexture(fbo2_texture);
            RS4P = null;
            fbo_main = 0;
            fbo2_main = 0;
            fbo_texture = 0;
            fbo2_texture = 0;
        }

        int fbo_texture;
        int fbo_main;
        int fbo2_texture;
        int fbo2_main;

        public void generateLightHelpers()
        {
            RS4P = new RenderSurface4Part(Window.Width, Window.Height, Rendering);
            // FBO
            fbo_texture = GL.GenTexture();
            fbo_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Width, Window.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            fbo2_texture = GL.GenTexture();
            fbo2_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo2_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Width, Window.Height, 0,
                OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo2_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo2_texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        Shader s_shadow;
        Shader s_main;
        Shader s_fbo;
        Shader s_shadowadder;
        Shader s_transponly;
        RenderSurface4Part RS4P;

        public float CameraFOV = 45f;

        public Location CameraUp = Location.UnitZ;

        public float CameraZNear = 0.1f;
        public float CameraZFar = 10000f;

        public Location ambient;

        void SetViewport()
        {
            vpw = Window.Width;
            vph = Window.Height;
            GL.Viewport(0, 0, vpw, vph);
        }

        public Location CameraPos;

        public Location CameraTarget;

        public bool RenderingShadows = false;

        public void sortEntities()
        {
            Entities = Entities.OrderBy(o => -(o.GetPosition() - CameraPos).LengthSquared()).ToList();
        }

        void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            gDelta = e.Time;
            try
            {
                RenderTextures = true;
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.1f, 0.1f, 0.1f, 1f });
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                GL.Enable(EnableCap.DepthTest);
                CameraPos = Player.GetPosition() + new Location(0, 0, Player.HalfSize.Z * 1.6f);
                sortEntities();
                Location CameraAngles = Player.GetAngles();
                double CameraYaw = CameraAngles.Yaw;
                double CameraPitch = CameraAngles.Pitch;
                if (CVars.r_lighting.ValueB)
                {
                    s_shadow.Bind();
                    VBO.BonesIdentity();
                    RenderingShadows = true;
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            Lights[i].InternalLights[x].Attach();
                            // TODO: Render settings
                            Render3D(true);
                            Lights[i].InternalLights[x].Complete();
                        }
                    }
                    SetViewport();
                    s_fbo.Bind();
                    VBO.BonesIdentity();
                    RenderingShadows = false;
                    CameraTarget = CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
                    Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFOV), (float)Window.Width / (float)Window.Height, CameraZNear, CameraZFar);
                    Matrix4 view = Matrix4.LookAt(CameraPos.ToOVector(), CameraTarget.ToOVector(), CameraUp.ToOVector());
                    Matrix4 combined = view * proj;
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    RS4P.Bind();
                    // TODO: Render settings
                    Render3D(false);
                    RS4P.Unbind();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
                    GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo2_main);
                    GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    s_shadowadder.Bind();
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.PositionTexture);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.NormalsTexture);
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
                    GL.ActiveTexture(TextureUnit.Texture5);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.RenderhintTexture);
                    GL.ActiveTexture(TextureUnit.Texture6);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                    Matrix4 mat = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
                    GL.UniformMatrix4(1, false, ref mat);
                    mat = Matrix4.Identity;
                    GL.UniformMatrix4(2, false, ref mat);
                    GL.Uniform3(10, CameraPos.ToOVector());
                    bool first = true;
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, first ? fbo_main : fbo2_main);
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                    GL.Disable(EnableCap.CullFace);
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, first ? fbo_main : fbo2_main);
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, first ? fbo2_texture : fbo_texture);
                            GL.ActiveTexture(TextureUnit.Texture4);
                            GL.BindTexture(TextureTarget.Texture2D, Lights[i].InternalLights[x].fbo_depthtex);
                            Matrix4 smat = Lights[i].InternalLights[x].GetMatrix();
                            GL.UniformMatrix4(3, false, ref smat);
                            GL.Uniform3(4, ref Lights[i].InternalLights[x].eye);
                            GL.Uniform3(8, ref Lights[i].InternalLights[x].color);
                            GL.Uniform1(9, Lights[i].InternalLights[x].maxrange);
                            Rendering.RenderRectangle(-1, -1, 1, 1);
                            first = !first;
                            GL.ActiveTexture(TextureUnit.Texture0);
                            GL.BindTexture(TextureTarget.Texture2D, 0);
                        }
                    }
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.DrawBuffer(DrawBufferMode.Back);
                    s_main.Bind();
                    GL.Uniform3(5, ambient.ToOVector());
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, first ? fbo2_texture : fbo_texture);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                    mat = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
                    GL.UniformMatrix4(1, false, ref mat);
                    mat = Matrix4.Identity;
                    GL.UniformMatrix4(2, false, ref mat);
                    Rendering.RenderRectangle(-1, -1, 1, 1);
                    GL.ActiveTexture(TextureUnit.Texture6);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture5);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture3);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    s_transponly.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
                    GL.BlitFramebuffer(0, 0, Window.Width, Window.Height, 0, 0, Window.Width, Window.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                    GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                    Render3D(false);
                    Shaders.ColorMultShader.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.Enable(EnableCap.CullFace);
                    if (CVars.r_renderwireframe.ValueB)
                    {
                        Render3DWires();
                    }
                }
                else
                {
                    Shaders.ColorMultShader.Bind();
                    Location CameraTarget = CameraPos + Utilities.ForwardVector_Deg(CameraYaw, CameraPitch);
                    Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CameraFOV), (float)Window.Width / (float)Window.Height, CameraZNear, CameraZFar);
                    Matrix4 view = Matrix4.LookAt(CameraPos.ToOVector(), CameraTarget.ToOVector(), CameraUp.ToOVector());
                    Matrix4 combined = view * proj;
                    GL.UniformMatrix4(1, false, ref combined);
                    Render3D(false);
                    if (CVars.r_renderwireframe.ValueB)
                    {
                        Render3DWires();
                    }
                }
                GL.Disable(EnableCap.DepthTest);
                Shaders.ColorMultShader.Bind();
                Ortho = Matrix4.CreateOrthographicOffCenter(0, Window.Width, Window.Height, 0, -1, 1);
                GL.UniformMatrix4(1, false, ref Ortho);
                Render2D();
            }
            catch (Exception ex)
            {
                SysConsole.Output(OutputType.ERROR, "Rendering: " + ex.ToString());
            }
            Window.SwapBuffers();
        }

        public void Render3D(bool shadows_only)
        {
            GL.Enable(EnableCap.CullFace);
            if (shadows_only)
            {
                for (int i = 0; i < ShadowCasters.Count; i++)
                {
                    ShadowCasters[i].Render();
                }
            }
            else
            {
                for (int i = 0; i < Entities.Count; i++)
                {
                    Entities[i].Render();
                }
                Rendering.SetMinimumLight(1.0f);
            }
            Textures.White.Bind();
            for (int i = 0; i < Joints.Count; i++)
            {
                if (Joints[i] is JointDistance)
                {
                    Rendering.RenderLine(((JointDistance)Joints[i]).Ent1Pos + Joints[i].Ent1.GetPosition(), ((JointDistance)Joints[i]).Ent2Pos + Joints[i].Ent2.GetPosition());
                }
                else
                {
                    Rendering.RenderLine(Joints[i].Ent1.GetPosition(), Joints[i].Ent2.GetPosition());
                }
            }
        }

        public bool RenderTextures = true;

        public void Render3DWires()
        {
            bool rt = RenderTextures;
            RenderTextures = false;
            Shaders.ColorMultShader.Bind();
            GL.Disable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            Textures.White.Bind();
            GL.Enable(EnableCap.CullFace);
            for (int i = 0; i < Entities.Count; i++)
            {
                Rendering.SetColor(Entities[i].Color);
                Entities[i].Render();
            }
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            Rendering.SetColor(Color4.White);
            RenderTextures = rt;
        }

        public void Render2D()
        {
            GL.Disable(EnableCap.CullFace);
            FontSets.Standard.DrawColoredText("^!^e^7gFPS(calc): " + (1f / gDelta) + "\n" + Player.GetPosition()
                + "\n" + Player.GetVelocity() + " == " + Player.GetVelocity().Length()
                + "\nLight source(s): " + Lights.Count
                + "\nEntities: " + Entities.Count, new Location(0, 0, 0));
            int center = Window.Width / 2;
            RenderItem(GetItemForSlot(QuickBarPos - 2), new Location(center - (32 + 32 + 32 + 3), Window.Height - (32 + 16), 0), 32);
            RenderItem(GetItemForSlot(QuickBarPos - 1), new Location(center - (32 + 32 + 2), Window.Height - (32 + 16), 0), 32);
            RenderItem(GetItemForSlot(QuickBarPos + 1), new Location(center + (32 + 1), Window.Height - (32 + 16), 0), 32);
            RenderItem(GetItemForSlot(QuickBarPos + 2), new Location(center + (32 + 32 + 2), Window.Height - (32 + 16), 0), 32);
            RenderItem(GetItemForSlot(QuickBarPos), new Location(center - (32 + 1), Window.Height - 64, 0), 64);
            string it = "^%^e^7" + GetItemForSlot(QuickBarPos).DisplayName;
            float size = FontSets.Standard.MeasureFancyText(it);
            FontSets.Standard.DrawColoredText(it, new Location(center - size / 2f, Window.Height - 64 - FontSets.Standard.font_default.Height - 5, 0));
            float percent = 0;
            if (Player.MaxHealth != 0)
            {
                percent = (float)Math.Round((Player.Health / Player.MaxHealth) * 10000) / 100f;
            }
            FontSets.Standard.DrawColoredText("^@^e^0" + Player.Health + "/" + Player.MaxHealth + " = " + percent + "%", new Location(5, Window.Height - FontSets.Standard.font_default.Height - 5, 0));
            UIConsole.Draw();
        }

        /// <summary>
        /// Renders an item on the 2D screen.
        /// </summary>
        /// <param name="item">The item to render</param>
        /// <param name="pos">Where to render it</param>
        /// <param name="size">How big to render it, in pixels</param>
        public void RenderItem(ItemStack item, Location pos, int size)
        {
            ItemFrame.Bind();
            Rendering.SetColor(Color4.White);
            Rendering.RenderRectangle((int)pos.X - 1, (int)pos.Y - 1, (int)(pos.X + size) + 1, (int)(pos.Y + size) + 1);
            item.Render(pos, new Location(size, size, 0));
            if (item.Count > 0)
            {
                FontSets.SlightlyBigger.DrawColoredText("^!^e^7^S" + item.Count, new Location(pos.X + 5, pos.Y + size - FontSets.SlightlyBigger.font_default.Height / 2f - 5, 0));
            }
        }

        public int vpw = 800;
        public int vph = 600;

        public bool RenderLights = false;

        public Matrix4 Ortho;
    }
}
