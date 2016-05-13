using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;
using Voxalia.ClientGame.UISystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.JointSystem;
using Voxalia.Shared.Collision;
using System.Diagnostics;
using FreneticScript;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double gDelta = 0;

        public ListQueue<VBO> vbos = new ListQueue<VBO>();

        public List<LightObject> Lights = new List<LightObject>();

        public void StandardBlend()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
        
        public void TranspBlend()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }
        
        void InitRendering()
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            vpw = Window.Width;
            vph = Window.Height;
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            StandardBlend();
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            ShadersCheck();
            generateLightHelpers();
            generateMapHelpers();
            generateTranspHelpers();
            skybox = new VBO[6];
            for (int i = 0; i < 6; i++)
            {
                skybox[i] = new VBO();
                skybox[i].Prepare();
            }
            skybox[0].AddSide(-Location.UnitZ, new TextureCoordinates());
            skybox[1].AddSide(Location.UnitZ, new TextureCoordinates());
            skybox[2].AddSide(-Location.UnitX, new TextureCoordinates());
            skybox[3].AddSide(Location.UnitX, new TextureCoordinates());
            skybox[4].AddSide(-Location.UnitY, new TextureCoordinates());
            skybox[5].AddSide(Location.UnitY, new TextureCoordinates());
            for (int i = 0; i < 6; i++)
            {
                skybox[i].GenerateVBO();
            }
        }

        public void ShadersCheck()
        {
            string def = CVars.r_good_graphics.ValueB ? "#MCM_GOOD_GRAPHICS" : "";
            s_shadow = Shaders.GetShader("shadow" + def);
            s_shadowvox = Shaders.GetShader("shadowvox" + def);
            s_fbo = Shaders.GetShader("fbo" + def);
            s_fbot = Shaders.GetShader("fbo" + def + ",MCM_TRANSP_ALLOWED");
            s_fbov = Shaders.GetShader("fbo_vox" + def);
            s_fbo_refract = Shaders.GetShader("fbo" + def + ",MCM_REFRACT");
            s_fbov_refract = Shaders.GetShader("fbo_vox" + def + ",MCM_REFRACT");
            s_shadowadder = Shaders.GetShader("shadowadder" + def);
            s_lightadder = Shaders.GetShader("lightadder" + def);
            s_transponly = Shaders.GetShader("transponly" + def);
            s_transponlyvox = Shaders.GetShader("transponlyvox" + def);
            s_transponlylit = Shaders.GetShader("transponly" + def + ",MCM_LIT");
            s_transponlyvoxlit = Shaders.GetShader("transponlyvox" + def + ",MCM_LIT");
            s_transponlylitsh = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS");
            s_transponlyvoxlitsh = Shaders.GetShader("transponlyvox" + def + ",MCM_LIT,MCM_SHADOWS");
            s_godray = Shaders.GetShader("godray" + def);
            s_mapvox = Shaders.GetShader("map_vox" + def);
            s_transpadder = Shaders.GetShader("transpadder" + def);
            s_finalgodray = Shaders.GetShader("finalgodray" + def);
            s_finalgodray_toonify = Shaders.GetShader("finalgodray" + def + ",MCM_TOONIFY");
        }

        int map_fbo_main;
        int map_fbo_texture;
        int map_fbo_depthtex;

        public void generateMapHelpers()
        {
            // TODO: Helper class!
            map_fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, map_fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 256, 256, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero); // TODO: Custom size!
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            map_fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, map_fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, 256, 256, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero); // TODO: Custom size!
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            map_fbo_main = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, map_fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, map_fbo_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, map_fbo_depthtex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        int transp_fbo_main;
        int transp_fbo_texture;
        int transp_fbo_depthtex;

        public void generateTranspHelpers()
        {
            // TODO: Helper class!
            transp_fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Width, Window.Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            transp_fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Window.Width, Window.Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            transp_fbo_main = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, transp_fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, transp_fbo_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, transp_fbo_depthtex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        VBO[] skybox;

        public void destroyLightHelpers()
        {
            RS4P.Destroy();
            GL.DeleteFramebuffer(fbo_main);
            GL.DeleteTexture(fbo_texture);
            RS4P = null;
            fbo_main = 0;
            fbo_texture = 0;
        }

        int fbo_texture;
        int fbo_main;

        int fbo_godray_main;
        int fbo_godray_texture;
        int fbo_godray_texture2;

        public void generateLightHelpers()
        {
            RS4P = new RenderSurface4Part(Window.Width, Window.Height, Rendering);
            // FBO
            fbo_texture = GL.GenTexture();
            fbo_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Width, Window.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            fbo_godray_texture = GL.GenTexture();
            fbo_godray_texture2 = GL.GenTexture();
            fbo_godray_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Width, Window.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Window.Width, Window.Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_godray_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, fbo_godray_texture2, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public Shader s_shadow;
        public Shader s_finalgodray;
        public Shader s_finalgodray_toonify;
        public Shader s_fbo;
        public Shader s_fbov;
        public Shader s_fbot;
        public Shader s_fbo_refract;
        public Shader s_fbov_refract;
        public Shader s_shadowadder;
        public Shader s_lightadder;
        public Shader s_transponly;
        public Shader s_transponlyvox;
        public Shader s_transponlylit;
        public Shader s_transponlyvoxlit;
        public Shader s_transponlylitsh;
        public Shader s_transponlyvoxlitsh;
        public Shader s_godray;
        public Shader s_shadowvox;
        public Shader s_mapvox;
        public Shader s_transpadder;
        RenderSurface4Part RS4P;

        public Location CameraUp = Location.UnitZ;

        public Location ambient;

        public float DesaturationAmount = 0f;

        public Location godrayCol = Location.One;

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
            TheRegion.Entities = TheRegion.Entities.OrderBy(o => (o.GetPosition() - CameraPos).LengthSquared()).ToList();
        }

        public void ReverseEntitiesOrder()
        {
            TheRegion.Entities.Reverse();
        }

        public int gTicks = 0;

        public int gFPS = 0;

        public Frustum CFrust = null;

        public int LightsC = 0;

        public byte FBOid = 0;

        int rTicks = 1000;

        public bool shouldRedrawShadows = false;

        public void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            lock (TickLock)
            {
                gDelta = e.Time;
                gTicks++;
                try
                {
                    Shaders.ColorMultShader.Bind();
                    GL.Uniform1(6, (float)GlobalTickTimeLocal);
                    CScreen.Render();
                    UIConsole.Draw();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Rendering (general): " + ex.ToString());
                }
                Stopwatch timer = new Stopwatch();
                try
                {
                    timer.Start();
                    tick(e.Time);
                    timer.Stop();
                    TickTime = (double)timer.ElapsedMilliseconds / 1000f;
                    if (TickTime > TickSpikeTime)
                    {
                        TickSpikeTime = TickTime;
                    }
                    timer.Reset();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Ticking: " + ex.ToString());
                }
                timer.Start();
                Window.SwapBuffers();
                timer.Stop();
                FinishTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (FinishTime > FinishSpikeTime)
                {
                    FinishSpikeTime = FinishTime;
                }
                timer.Reset();
            }
        }

        public double ShadowTime;
        public double TickTime;
        public double FBOTime;
        public double LightsTime;
        public double FinishTime;
        public double TWODTime;
        public double ShadowSpikeTime;
        public double TickSpikeTime;
        public double FBOSpikeTime;
        public double LightsSpikeTime;
        public double FinishSpikeTime;
        public double TWODSpikeTime;
        
        public double mapLastRendered = 0;

        public const float LightMaximum = 1E10f;

        public void renderGame()
        {
            Stopwatch timer = new Stopwatch();
            try
            {
                StandardBlend();
                RenderTextures = true;
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f, 0f, 1f, 1f });
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                GL.Enable(EnableCap.DepthTest);
                if (CVars.g_firstperson.ValueB)
                {
                    CameraPos = PlayerEyePosition;
                }
                else
                {
                    CollisionResult cr = TheRegion.Collision.RayTrace(PlayerEyePosition, PlayerEyePosition - Player.ForwardVector() * 2, Player.IgnoreThis);
                    if (cr.Hit)
                    {
                        CameraPos = cr.Position + cr.Normal * 0.05;
                    }
                    else
                    {
                        CameraPos = cr.Position;
                    }
                }
                if (CVars.u_showmap.ValueB && mapLastRendered + 1.0 < TheRegion.GlobalTickTimeLocal) // TODO: 1.0 -> custom
                {
                    mapLastRendered = TheRegion.GlobalTickTimeLocal;
                    AABB box = new AABB() { Min = Player.GetPosition(), Max = Player.GetPosition() };
                    foreach (Chunk ch in TheRegion.LoadedChunks.Values)
                    {
                        box.Include(ch.WorldPosition * Chunk.CHUNK_SIZE);
                        box.Include(ch.WorldPosition * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE));
                    }
                    Matrix4 ortho = Matrix4.CreateOrthographicOffCenter((float)box.Min.X, (float)box.Max.X, (float)box.Min.Y, (float)box.Max.Y, (float)box.Min.Z, (float)box.Max.Z);
                    Matrix4 oident = Matrix4.Identity;
                    s_mapvox = s_mapvox.Bind();
                    GL.UniformMatrix4(1, false, ref ortho);
                    GL.Viewport(0, 0, 256, 256); // TODO: Customizable!
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, map_fbo_main);
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                    GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 1.0f, 0.0f, 1.0f });
                    GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                    GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                    foreach (Chunk chunk in TheRegion.LoadedChunks.Values)
                    {
                        chunk.Render();
                    }
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.BindTexture(TextureTarget.Texture2DArray, 0);
                    GL.DrawBuffer(DrawBufferMode.Back);
                }
                sortEntities();
                SetViewport();
                CameraTarget = CameraPos + Player.ForwardVector();
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CVars.r_fov.ValueF), (float)Window.Width / (float)Window.Height, CVars.r_znear.ValueF, CVars.r_zfar.ValueF);
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(CameraPos), ClientUtilities.Convert(CameraTarget), ClientUtilities.Convert(CameraUp));
                Matrix4 combined = view * proj;
                Frustum camFrust = new Frustum(combined);
                if (shouldRedrawShadows && CVars.r_shadows.ValueB)
                {
                    timer.Start();
                    shouldRedrawShadows = false;
                    s_shadow = s_shadow.Bind();
                    VBO.BonesIdentity();
                    RenderingShadows = true;
                    LightsC = 0;
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                        {
                            if (Lights[i] is SkyLight || (Lights[i].EyePos - CameraPos).LengthSquared() < CVars.r_lightmaxdistance.ValueD * CVars.r_lightmaxdistance.ValueD + Lights[i].MaxDistance * Lights[i].MaxDistance * 6)
                            {
                                LightsC++;
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        CFrust = null;
                                    }
                                    else
                                    {
                                        CFrust = new Frustum(Lights[i].InternalLights[x].GetMatrix());
                                    }
                                    s_shadowvox = s_shadowvox.Bind();
                                    Matrix4 tident = Matrix4.Identity;
                                    GL.UniformMatrix4(2, false, ref tident);
                                    Lights[i].InternalLights[x].SetProj();
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        GL.Uniform1(3, 1.0f);
                                    }
                                    else
                                    {
                                        GL.Uniform1(3, 0.0f);
                                    }
                                    FBOid = 4;
                                    s_shadow = s_shadow.Bind();
                                    GL.UniformMatrix4(2, false, ref tident);
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        GL.Uniform1(3, 1.0f);
                                    }
                                    else
                                    {
                                        GL.Uniform1(3, 0.0f);
                                    }
                                    Lights[i].InternalLights[x].Attach();
                                    // TODO: Render settings
                                    Render3D(true);
                                    FBOid = 0;
                                    Lights[i].InternalLights[x].Complete();
                                }
                            }
                        }
                    }
                    timer.Stop();
                    ShadowTime = (double)timer.ElapsedMilliseconds / 1000f;
                    if (ShadowTime > ShadowSpikeTime)
                    {
                        ShadowSpikeTime = ShadowTime;
                    }
                    timer.Reset();
                }
                timer.Start();
                SetViewport();
                Matrix4 matident = Matrix4.Identity;
                s_fbov = s_fbov.Bind();
                GL.Uniform1(6, (float)GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.Uniform2(8, new Vector2(sl_min, sl_max));
                s_fbot = s_fbot.Bind();
                GL.Uniform1(6, (float)GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                s_fbo = s_fbo.Bind();
                GL.Uniform1(6, (float)GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                FBOid = 1;
                RenderingShadows = false;
                CFrust = camFrust;
                GL.ActiveTexture(TextureUnit.Texture0);
                RS4P.Bind();
                RenderLights = true;
                RenderSpecular = true;
                Rendering.SetColor(Color4.White);
                VBO.BonesIdentity();
                StandardBlend();
                Render3D(false);
                FBOid = 21;
                s_fbov_refract = s_fbov_refract.Bind();
                GL.Uniform1(6, (float)GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.Uniform2(8, new Vector2(sl_min, sl_max));
                s_fbo_refract = s_fbo_refract.Bind();
                GL.Uniform1(6, (float)GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.DepthMask(false);
                Render3D(false);
                GL.DepthMask(true);
                RenderLights = false;
                RenderSpecular = false;
                RS4P.Unbind();
                FBOid = 0;
                timer.Stop();
                FBOTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (FBOTime > FBOSpikeTime)
                {
                    FBOSpikeTime = FBOTime;
                }
                timer.Reset();
                timer.Start();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                Shader s_other;
                if (CVars.r_shadows.ValueB)
                {
                    s_other = s_shadowadder;
                    s_other.Bind();
                    GL.Uniform1(13, CVars.r_shadowblur.ValueF);
                }
                else
                {
                    s_other = s_lightadder;
                }
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
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.Rh2Texture);
                Matrix4 mat = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
                s_other.Bind();
                GL.UniformMatrix4(1, false, ref mat);
                GL.UniformMatrix4(2, false, ref matident);
                GL.Uniform3(10, ClientUtilities.Convert(CameraPos));
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                TranspBlend();
                if (CVars.r_lighting.ValueB)
                {
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                        {
                            s_other.Bind();
                            double d1 = (Lights[i].EyePos - CameraPos).LengthSquared();
                            double d2 = CVars.r_lightmaxdistance.ValueD * CVars.r_lightmaxdistance.ValueD + Lights[i].MaxDistance * Lights[i].MaxDistance;
                            double maxrangemult = 0;
                            if (d1 < d2 * 4 || Lights[i] is SkyLight)
                            {
                                maxrangemult = 1;
                            }
                            else if (d1 < d2 * 6)
                            {
                                maxrangemult = 1 - ((d1 - (d2 * 4)) / ((d2 * 6) - (d2 * 4)));
                            }
                            if (maxrangemult > 0)
                            {
                                GL.Uniform1(11, Lights[i] is SpotLight ? 1f : 0f);
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    if (CVars.r_shadows.ValueB)
                                    {
                                        GL.ActiveTexture(TextureUnit.Texture4);
                                        GL.BindTexture(TextureTarget.Texture2D, Lights[i].InternalLights[x].fbo_depthtex);
                                    }
                                    Matrix4 smat = Lights[i].InternalLights[x].GetMatrix();
                                    GL.UniformMatrix4(3, false, ref smat);
                                    GL.Uniform3(4, ref Lights[i].InternalLights[x].eye);
                                    Vector3 col = Lights[i].InternalLights[x].color * (float)maxrangemult;
                                    GL.Uniform3(8, ref col);
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        GL.Uniform1(9, LightMaximum);
                                        GL.Uniform1(7, 1.0f);
                                    }
                                    else
                                    {
                                        float range = Lights[i].InternalLights[0].maxrange;
                                        GL.Uniform1(9, range <= 0 ? LightMaximum : range);
                                        GL.Uniform1(7, 0.0f);
                                    }
                                    if (CVars.r_shadows.ValueB)
                                    {
                                        GL.Uniform1(12, 1f / Lights[i].InternalLights[x].texsize);
                                    }
                                    Rendering.RenderRectangle(-1, -1, 1, 1);
                                    GL.ActiveTexture(TextureUnit.Texture0);
                                    GL.BindTexture(TextureTarget.Texture2D, 0);
                                }
                            }
                        }
                    }
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
                if (CVars.r_toonify.ValueB)
                {
                    s_finalgodray_toonify = s_finalgodray_toonify.Bind();
                }
                else
                {
                    s_finalgodray = s_finalgodray.Bind();
                }
                GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
                GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 1f, 1f, 1f, 0f });
                GL.BlendFuncSeparate(1, BlendingFactorSrc.SrcColor, BlendingFactorDest.Zero, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
                GL.Uniform1(19, DesaturationAmount);
                GL.Uniform3(5, ClientUtilities.Convert(CVars.r_lighting.ValueB ? ambient : new Location(1, 1, 1)));
                GL.Uniform3(8, ClientUtilities.Convert(CameraFinalTarget));
                GL.Uniform1(9, CVars.r_dof_strength.ValueF);
                Vector3 lPos = GetSunLocation();
                Vector4 t = Vector4.Transform(new Vector4(lPos, 1f), combined);
                Vector2 lp1 = t.Xy / t.W;
                Vector2 lightPos = lp1 * 0.5f + new Vector2(0.5f);
                float lplenadj = (1f - Math.Min(lp1.Length, 1f)) * (0.99f - 0.6f) + 0.6f;
                GL.Uniform2(10, ref lightPos);
                GL.Uniform1(11, CVars.r_godray_samples.ValueI);
                GL.Uniform1(12, CVars.r_godray_wexposure.ValueF);
                GL.Uniform1(13, CVars.r_godray_decay.ValueF);
                GL.Uniform1(14, CVars.r_godray_density.ValueF * lplenadj);
                GL.Uniform3(15, ClientUtilities.Convert(godrayCol));
                GL.Uniform1(16, CVars.r_znear.ValueF);
                GL.Uniform1(17, CVars.r_zfar.ValueF);
                Material headmat = TheRegion.GetBlockMaterial(CameraPos);
                GL.Uniform4(18, new Vector4(ClientUtilities.Convert(headmat.GetFogColor()), headmat.GetFogAlpha()));
                GL.Uniform3(20, ClientUtilities.Convert(CameraPos));
                GL.Uniform1(21, CVars.r_znear.ValueF);
                GL.Uniform1(23, CVars.r_zfar.ValueF);
                GL.Uniform1(24, (float)Window.Width);
                GL.Uniform1(25, (float)Window.Height);
                GL.UniformMatrix4(22, false, ref combined);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.bwtexture);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                GL.UniformMatrix4(1, false, ref mat);
                GL.UniformMatrix4(2, false, ref matident);
                Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.Texture2D, 0);
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
                GL.Enable(EnableCap.DepthTest);
                GL.BlendFunc(1, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.DrawBuffer(DrawBufferMode.Back);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo); // TODO: is this line and line below needed?
                GL.BlitFramebuffer(0, 0, Window.Width, Window.Height, 0, 0, Window.Width, Window.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo_godray_main);
                GL.BlitFramebuffer(0, 0, Window.Width, Window.Height, 0, 0, Window.Width, Window.Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                Matrix4 def = Matrix4.Identity;
                GL.Enable(EnableCap.CullFace);
                ReverseEntitiesOrder();
                Particles.Sort();
                if (CVars.r_transplighting.ValueB)
                {
                    if (CVars.r_transpshadows.ValueB && CVars.r_shadows.ValueB)
                    {
                        s_transponlyvoxlitsh =s_transponlyvoxlitsh.Bind();
                    }
                    else
                    {
                        s_transponlyvoxlit = s_transponlyvoxlit.Bind();
                    }
                }
                else
                {
                    s_transponlyvox = s_transponlyvox.Bind();
                }
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                GL.Uniform1(4, DesaturationAmount);
                if (CVars.r_transplighting.ValueB)
                {
                    if (CVars.r_transpshadows.ValueB && CVars.r_shadows.ValueB)
                    {
                        s_transponlylitsh = s_transponlylitsh.Bind();
                        FBOid = 8;
                    }
                    else
                    {
                        s_transponlylit = s_transponlylit.Bind();
                        FBOid = 7;
                    }
                }
                else
                {
                    s_transponly = s_transponly.Bind();
                    FBOid = 3;
                }
                VBO.BonesIdentity();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                GL.Uniform1(4, DesaturationAmount);
                GL.DepthMask(false);
                TranspBlend();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, transp_fbo_main);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
                GL.BlitFramebuffer(0, 0, Window.Width, Window.Height, 0, 0, Window.Width, Window.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
                int lightc = 0;
                if (CVars.r_transplighting.ValueB)
                {
                    RenderLights = true;
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                        {
                            for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                            {
                                lightc++;
                            }
                        }
                    }
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                        {
                            for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                            {
                                if (CVars.r_transpshadows.ValueB && CVars.r_shadows.ValueB)
                                {
                                    s_transponlyvoxlitsh = s_transponlyvoxlitsh.Bind();
                                    GL.ActiveTexture(TextureUnit.Texture4);
                                    GL.BindTexture(TextureTarget.Texture2D, Lights[i].InternalLights[x].fbo_depthtex);
                                    GL.ActiveTexture(TextureUnit.Texture0);
                                }
                                else
                                {
                                    s_transponlyvoxlit = s_transponlyvoxlit.Bind();
                                }
                                Matrix4 lmat = Lights[i].InternalLights[x].GetMatrix();
                                GL.UniformMatrix4(6, false, ref lmat);
                                GL.Uniform3(7, Lights[i].InternalLights[x].color);
                                float maxrange = (Lights[i].InternalLights[x] is LightOrtho) ? LightMaximum : Lights[i].InternalLights[x].maxrange;
                                Matrix4 matxyz = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                                matxyz[0, 0] = maxrange <= 0 ? LightMaximum : maxrange;
                                // TODO: Diffuse Albedo
                                matxyz[0, 1] = 0.7f;
                                matxyz[0, 2] = 0.7f;
                                matxyz[0, 3] = 0.7f;
                                // TODO: Specular Albedo
                                matxyz[1, 0] = 0.7f;
                                matxyz[1, 3] = (Lights[i] is SpotLight) ? 1f : 0f;
                                matxyz[2, 0] = (Lights[i].InternalLights[x] is LightOrtho) ? 1f : 0f;
                                matxyz[2, 1] = 1f / Lights[i].InternalLights[x].texsize;
                                matxyz[2, 2] = 0.5f;
                                matxyz[2, 3] = (float)lightc;
                                matxyz[3, 0] = (float)ambient.X;
                                matxyz[3, 1] = (float)ambient.Y;
                                matxyz[3, 2] = (float)ambient.Z;
                                GL.UniformMatrix4(8, false, ref matxyz);
                                Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                                matabc[0, 0] = (float)CameraPos.X;
                                matabc[0, 1] = (float)CameraPos.Y;
                                matabc[0, 2] = (float)CameraPos.Z;
                                matabc[1, 0] = (float)Lights[i].EyePos.X;
                                matabc[1, 1] = (float)Lights[i].EyePos.Y;
                                matabc[1, 2] = (float)Lights[i].EyePos.Z;
                                GL.UniformMatrix4(9, false, ref matabc);
                                if (CVars.r_transpshadows.ValueB && CVars.r_shadows.ValueB)
                                {
                                    s_transponlylitsh = s_transponlylitsh.Bind();
                                }
                                else
                                {
                                    s_transponlylit = s_transponlylit.Bind();
                                }
                                GL.Uniform3(5, Lights[i].InternalLights[x].eye);
                                GL.UniformMatrix4(6, false, ref lmat);
                                GL.Uniform3(7, Lights[i].InternalLights[x].color);
                                GL.UniformMatrix4(8, false, ref matxyz);
                                GL.UniformMatrix4(9, false, ref matabc);
                                Render3D(false);
                            }
                        }
                    }
                    GL.ActiveTexture(TextureUnit.Texture4);
                    GL.BindTexture(TextureTarget.Texture2D, 0);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    RenderLights = false;
                }
                else
                {
                    Render3D(false);
                }
                if (lightc == 0)
                {
                    lightc = 1;
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.DrawBuffer(DrawBufferMode.Back);
                StandardBlend();
                FBOid = 0;
                GL.ActiveTexture(TextureUnit.Texture0);
                if (CVars.r_godrays.ValueB)
                {
                    GL.Disable(EnableCap.CullFace);
                    GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                    GL.Disable(EnableCap.DepthTest);
                    GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
                    s_godray = s_godray.Bind();
                    GL.UniformMatrix4(1, false, ref mat);
                    GL.UniformMatrix4(2, false, ref matident);
                    TranspBlend();
                    Rendering.RenderRectangle(-1, -1, 1, 1);
                    StandardBlend();
                }
                {
                    GL.Disable(EnableCap.CullFace);
                    GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                    GL.Enable(EnableCap.DepthTest);
                    GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
                    s_transpadder = s_transpadder.Bind();
                    GL.UniformMatrix4(1, false, ref mat);
                    GL.UniformMatrix4(2, false, ref matident);
                    GL.Uniform1(3, (float)lightc);
                    Rendering.RenderRectangle(-1, -1, 1, 1);
                }
                GL.UseProgram(0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.DepthMask(true);
                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                timer.Stop();
                LightsTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (LightsTime > LightsSpikeTime)
                {
                    LightsSpikeTime = LightsTime;
                }
                timer.Reset();
            }
            catch (Exception ex)
            {
                SysConsole.Output("Rendering (3D)", ex);
            }
            try
            {
                timer.Start();
                Establish2D();
                Render2D();
                timer.Stop();
                TWODTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (TWODTime > TWODSpikeTime)
                {
                    TWODSpikeTime = TWODTime;
                }
                timer.Reset();
            }
            catch (Exception ex)
            {
                SysConsole.Output("Rendering (2D)", ex);
            }
        }

        float dist2 = 750; // TODO: (View rad + 2) * CHUNK_SIZE ? Or base off ZFAR?
        float dist = 670;

        public bool RenderSpecular = false;

        public Vector3 GetSunLocation()
        {
            return ClientUtilities.Convert(CameraPos + TheSun.Direction * -(dist * 0.96f));
        }

        public void RenderSkybox()
        {
            if (FBOid == 1)
            {
                GL.Uniform4(7, new Vector4(0f, 0f, 0f, 0f));
            }
            Rendering.SetMinimumLight(1);
            GL.Disable(EnableCap.CullFace);
            Rendering.SetColor(Color4.White);
            Matrix4 scale = Matrix4.CreateScale(dist2, dist2, dist2) * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos));
            GL.UniformMatrix4(2, false, ref scale);
            // TODO: Save textures!
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/bottom").Bind();
            skybox[0].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/top").Bind();
            skybox[1].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/xm").Bind();
            skybox[2].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/xp").Bind();
            skybox[3].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/ym").Bind();
            skybox[4].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "_night/yp").Bind();
            skybox[5].Render(false);
            Rendering.SetColor(new Vector4(1, 1, 1, (float)Math.Max(Math.Min((SunAngle.Pitch - 70) / (-90), 1), 0)));
            scale = Matrix4.CreateScale(dist, dist, dist) * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos));
            GL.UniformMatrix4(2, false, ref scale);
            // TODO: Save textures!
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/bottom").Bind();
            skybox[0].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/top").Bind();
            skybox[1].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/xm").Bind();
            skybox[2].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/xp").Bind();
            skybox[3].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/ym").Bind();
            skybox[4].Render(false);
            Textures.GetTexture("skies/" + CVars.r_skybox.Value + "/yp").Bind();
            skybox[5].Render(false);
            if (FBOid == 1)
            {
                GL.Uniform4(7, Color4.White);
            }
            Rendering.SetColor(new Vector4(ClientUtilities.Convert(godrayCol), 1));
            Textures.GetTexture("skies/sun").Bind(); // TODO: Store var!
            Matrix4 rot = Matrix4.CreateTranslation(-50f, -50f, 0f)
                * Matrix4.CreateRotationY((float)((-SunAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + SunAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos + TheSun.Direction * -(dist * 0.96f)));
            Rendering.RenderRectangle(0, 0, 100, 100, rot); // TODO: Adjust scale based on view rad
            if (FBOid == 1)
            {
                GL.Uniform4(7, Color4.Black);
            }
            Textures.GetTexture("skies/planet").Bind(); // TODO: Store var!
            Rendering.SetColor(new Color4(PlanetLight, PlanetLight, PlanetLight, 1));
            rot = Matrix4.CreateTranslation(-150f, -150f, 0f)
                * Matrix4.CreateRotationY((float)((-PlanetAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + PlanetAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos + ThePlanet.Direction * -(dist * 0.8f)));
            Rendering.RenderRectangle(0, 0, 300, 300, rot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.CullFace);
            Matrix4 ident = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref ident);
            Rendering.SetColor(Color4.White);
            if (FBOid == 3 || FBOid == 7 || FBOid == 8)
            {
                TheRegion.RenderClouds();
            }
            Rendering.SetMinimumLight(0);
        }
        
        public void Establish2D()
        {
            GL.Disable(EnableCap.DepthTest);
            Shaders.ColorMultShader.Bind();
            Ortho = Matrix4.CreateOrthographicOffCenter(0, Window.Width, Window.Height, 0, -1, 1);
            GL.UniformMatrix4(1, false, ref Ortho);
        }

        public void Render3D(bool shadows_only)
        {
            if (FBOid == 1)
            {
                GL.Uniform4(7, Color4.Black);
            }
            GL.Enable(EnableCap.CullFace);
            if (shadows_only)
            {
                for (int i = 0; i < TheRegion.ShadowCasters.Count; i++)
                {
                    TheRegion.ShadowCasters[i].Render();
                }
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture3);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                if (FBOid == 1)
                {
                    s_fbot.Bind();
                    RenderSkybox();
                    s_fbo.Bind();
                }
                for (int i = 0; i < TheRegion.Entities.Count; i++)
                {
                    TheRegion.Entities[i].Render();
                }
                if (FBOid == 1)
                {
                    Rendering.SetMinimumLight(1f);
                }
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture3);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                Particles.Engine.Render();
            }
            if (FBOid == 1)
            {
                s_fbov = s_fbov.Bind();
                GL.Uniform4(7, Color4.Black);
            }
            if (FBOid == 21)
            {
                s_fbov_refract = s_fbov_refract.Bind();
            }
            else if (FBOid == 3)
            {
                s_transponlyvox = s_transponlyvox.Bind();
            }
            else if (FBOid == 7)
            {
                s_transponlyvoxlit = s_transponlyvoxlit.Bind();
            }
            else if (FBOid == 8)
            {
                s_transponlyvoxlitsh = s_transponlyvoxlitsh.Bind();
            }
            else if (FBOid == 4)
            {
                s_shadowvox = s_shadowvox.Bind();
            }
            TheRegion.Render();
            if (FBOid == 1)
            {
                s_fbo = s_fbo.Bind();
                GL.Uniform4(7, Color4.Black);
            }
            else if (FBOid == 1)
            {
                s_fbo_refract = s_fbo_refract.Bind();
            }
            else if (FBOid == 3)
            {
                s_transponly = s_transponly.Bind();
            }
            else if (FBOid == 7)
            {
                s_transponlylit = s_transponlylit.Bind();
            }
            else if (FBOid == 8)
            {
                s_transponlylitsh = s_transponlylitsh.Bind();
            }
            else if (FBOid == 4)
            {
                s_shadow = s_shadow.Bind();
            }
            if (!shadows_only)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture2);
                Textures.Black.Bind();
                GL.ActiveTexture(TextureUnit.Texture3);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            Textures.White.Bind();
            Location mov = (CameraFinalTarget - CameraPos) / CameraDistance;
            Location cpos = CameraFinalTarget - (CameraImpactNormal * 0.01f);
            Location cpos2 = CameraFinalTarget + (CameraImpactNormal * 0.91f);
            // TODO: 5 -> Variable length (Server controlled?)
            if (TheRegion.GetBlockMaterial(cpos) != Material.AIR && CameraDistance < 5)
            {
                if (CVars.u_highlight_targetblock.ValueB)
                {
                    Location cft = cpos.GetBlockLocation();
                    GL.LineWidth(3);
                    Rendering.SetColor(Color4.Blue);
                    Rendering.SetMinimumLight(1.0f);
                    Rendering.RenderLineBox(cft - mov * 0.01f, cft + Location.One - mov * 0.01f);
                    GL.LineWidth(1);
                }
                if (CVars.u_highlight_placeblock.ValueB)
                {
                    Rendering.SetColor(Color4.Cyan);
                    Location cft2 = cpos2.GetBlockLocation();
                    Rendering.RenderLineBox(cft2, cft2 + Location.One);
                }
                Rendering.SetColor(Color4.White);
            }
            if (FBOid == 1)
            {
                Rendering.SetMinimumLight(0f);
            }
            if (CVars.n_debugmovement.ValueB)
            {
                Rendering.SetColor(Color4.Red);
                GL.LineWidth(5);
                foreach (Chunk chunk in TheRegion.LoadedChunks.Values)
                {
                    if (chunk._VBO == null && !chunk.IsAir)
                    {
                        Rendering.RenderLineBox(chunk.WorldPosition * Chunk.CHUNK_SIZE, (chunk.WorldPosition + Location.One) * Chunk.CHUNK_SIZE);
                    }
                }
                GL.LineWidth(1);
                Rendering.SetColor(Color4.White);
            }
            Textures.White.Bind();
            Rendering.SetMinimumLight(1);
            TheRegion.RenderEffects();
            Textures.GetTexture("effects/beam").Bind(); // TODO: Store
            for (int i = 0; i < TheRegion.Joints.Count; i++)
            {
                if (TheRegion.Joints[i] is ConnectorBeam)
                {
                    switch (((ConnectorBeam)TheRegion.Joints[i]).type)
                    {
                        case BeamType.STRAIGHT:
                            {
                                Location one = TheRegion.Joints[i].One.GetPosition();
                                if (TheRegion.Joints[i].One is CharacterEntity)
                                {
                                    one = ((CharacterEntity)TheRegion.Joints[i].One).GetEyePosition() + new Location(0, 0, -0.3);
                                }
                                Location two = TheRegion.Joints[i].Two.GetPosition();
                                Vector4 col = Rendering.AdaptColor(ClientUtilities.Convert((one + two) * 0.5), ((ConnectorBeam)TheRegion.Joints[i]).color);
                                Rendering.SetColor(col);
                                Rendering.RenderLine(one, two);
                            }
                            break;
                        case BeamType.CURVE:
                            {
                                Location one = TheRegion.Joints[i].One.GetPosition();
                                Location two = TheRegion.Joints[i].Two.GetPosition();
                                Location cPoint = (one + two) * 0.5f;
                                if (TheRegion.Joints[i].One is CharacterEntity)
                                {
                                    one = ((CharacterEntity)TheRegion.Joints[i].One).GetEyePosition() + new Location(0, 0, -0.3);
                                    cPoint = one + ((CharacterEntity)TheRegion.Joints[i].One).ForwardVector() * (two - one).Length();
                                }
                                DrawCurve(one, two, cPoint, ((ConnectorBeam)TheRegion.Joints[i]).color);
                            }
                            break;
                        case BeamType.MULTICURVE:
                            {
                                Location one = TheRegion.Joints[i].One.GetPosition();
                                Location two = TheRegion.Joints[i].Two.GetPosition();
                                double forlen = 1;
                                Location forw = Location.UnitZ;
                                if (TheRegion.Joints[i].One is CharacterEntity)
                                {
                                    one = ((CharacterEntity)TheRegion.Joints[i].One).GetEyePosition() + new Location(0, 0, -0.3);
                                    forlen = (two - one).Length();
                                    forw = ((CharacterEntity)TheRegion.Joints[i].One).ForwardVector();
                                }
                                Location spos = one + forw * forlen;
                                const int curves = 5;
                                BEPUutilities.Vector3 bvec = new BEPUutilities.Vector3(0, 0, 1);
                                BEPUutilities.Vector3 bvec2 = new BEPUutilities.Vector3(1, 0, 0);
                                BEPUutilities.Quaternion bquat;
                                BEPUutilities.Quaternion.GetQuaternionBetweenNormalizedVectors(ref bvec2, ref bvec, out bquat);
                                BEPUutilities.Vector3 forwvec = forw.ToBVector();
                                GL.LineWidth(6);
                                DrawCurve(one, two, spos, ((ConnectorBeam)TheRegion.Joints[i]).color);
                                for (int c = 0; c < curves; c++)
                                {
                                    double tang = TheRegion.GlobalTickTimeLocal + Math.PI * 2.0 * ((double)c / (double)curves);
                                    BEPUutilities.Vector3 res = BEPUutilities.Quaternion.Transform(forw.ToBVector(), bquat);
                                    BEPUutilities.Quaternion quat = BEPUutilities.Quaternion.CreateFromAxisAngle(forw.ToBVector(), (float)(tang % (Math.PI * 2.0)));
                                    res = BEPUutilities.Quaternion.Transform(res, quat);
                                    res = res * (float)(0.1 * forlen);
                                    DrawCurve(one, two, spos + new Location(res), ((ConnectorBeam)TheRegion.Joints[i]).color);
                                }
                            }
                            break;
                    }
                }
            }
            Rendering.SetMinimumLight(0);
            Textures.White.Bind();
            if (!shadows_only)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture3);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        void DrawCurve(Location one, Location two, Location cPoint, System.Drawing.Color color)
        {
            const int curvePoints = 10;
            const double step = 1.0 / curvePoints;
            Location curvePos = one;
            for (double t = step; t <= 1.0; t += step)
            {
                Vector4 col = Rendering.AdaptColor(ClientUtilities.Convert(cPoint), color);
                Rendering.SetColor(col);
                Location c2 = CalculateBezierPoint(t, one, cPoint, two);
                Rendering.RenderBilboardLine(curvePos, c2, 3, CameraPos);
                curvePos = c2;
            }
        }

        Location CalculateBezierPoint(double t, Location p0, Location p1, Location p2)
        {
            double u = 1 - t;
            return (u * u) * p0 + 2 * u * t * p1 + t * t * p2;
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
            for (int i = 0; i < TheRegion.Entities.Count; i++)
            {
                Rendering.SetColor(TheRegion.Entities[i].WireColor);
                TheRegion.Entities[i].Render();
            }
            Rendering.SetColor(Color4.White);
            TheRegion.Render();
            // TODO: Render joints?
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            Rendering.SetColor(Color4.White);
            RenderTextures = rt;
        }

        public double RenderExtraItems = 0;

        const string timeformat = "#.000";

        const string healthformat = "0.0";

        const string pingformat = "000";

        public int ChunksRenderingCurrently = 0;

        public void Render2D()
        {
            GL.Disable(EnableCap.CullFace);
            if (CVars.u_showhud.ValueB && CInvMenu == null)
            {
                if (CVars.u_showping.ValueB)
                {
                    string pingdetail = "^0^e^&ping: " + (Math.Max(LastPingValue, GlobalTickTimeLocal - LastPingTime) * 1000.0).ToString(pingformat) + "ms";
                    string pingdet2 = "^0^e^&average: " + (APing * 1000.0).ToString(pingformat) + "ms";
                    FontSets.Standard.DrawColoredText(pingdetail, new Location(Window.Width - FontSets.Standard.MeasureFancyText(pingdetail), Window.Height - FontSets.Standard.font_default.Height * 2, 0));
                    FontSets.Standard.DrawColoredText(pingdet2, new Location(Window.Width - FontSets.Standard.MeasureFancyText(pingdet2), Window.Height - FontSets.Standard.font_default.Height, 0));
                }
                if (CVars.u_debug.ValueB)
                {
                    FontSets.Standard.DrawColoredText(FontSets.Standard.SplitAppropriately("^!^e^7gFPS(calc): " + (1f / gDelta) + ", gFPS(actual): " + gFPS
                        + "\nHeld Item: " + GetItemForSlot(QuickBarPos).ToString()
                        + "\nTimes -> Phyiscs: " + TheRegion.PhysTime.ToString(timeformat) + ", Shadows: " + ShadowTime.ToString(timeformat)
                        + ", FBO: " + FBOTime.ToString(timeformat) + ", Lights: " + LightsTime.ToString(timeformat) + ", 2D: " + TWODTime.ToString(timeformat)
                        + ", Tick: " + TickTime.ToString(timeformat) + ", Finish: " + FinishTime.ToString(timeformat)
                        + "\nSpike Times -> Shadows: " + ShadowSpikeTime.ToString(timeformat)
                        + ", FBO: " + FBOSpikeTime.ToString(timeformat) + ", Lights: " + LightsSpikeTime.ToString(timeformat) + ", 2D: " + TWODSpikeTime.ToString(timeformat)
                        + ", Tick: " + TickSpikeTime.ToString(timeformat) + ", Finish: " + FinishSpikeTime.ToString(timeformat)
                        + "\nChunks loaded: " + TheRegion.LoadedChunks.Count + ", Chunks rendering currently: " + ChunksRenderingCurrently + ", Entities loaded: " + TheRegion.Entities.Count
                        + "\nPosition: " + Player.GetPosition().ToBasicString() + ", velocity: " + Player.GetVelocity().ToBasicString() + ", direction: " + Player.Direction.ToBasicString(),
                        Window.Width - 10), new Location(0, 0, 0));
                }
                int center = Window.Width / 2;
                int bottomup = 32;
                if (RenderExtraItems > 0)
                {
                    RenderExtraItems -= gDelta;
                    if (RenderExtraItems < 0)
                    {
                        RenderExtraItems = 0;
                    }
                    RenderItem(GetItemForSlot(QuickBarPos - 5), new Location(center - (32 + 32 + 32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16 + bottomup), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos - 4), new Location(center - (32 + 32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16 + bottomup), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos - 3), new Location(center - (32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16 + bottomup), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 3), new Location(center + (32 + 32 + 32 + 2), Window.Height - (32 + 16 + bottomup), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 4), new Location(center + (32 + 32 + 32 + 32 + 2), Window.Height - (32 + 16 + bottomup), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 5), new Location(center + (32 + 32 + 32 + 32 + 32 + 2), Window.Height - (32 + 16 + bottomup), 0), 32);
                }
                RenderItem(GetItemForSlot(QuickBarPos - 2), new Location(center - (32 + 32 + 32 + 3), Window.Height - (32 + 16 + bottomup), 0), 32);
                RenderItem(GetItemForSlot(QuickBarPos - 1), new Location(center - (32 + 32 + 2), Window.Height - (32 + 16 + bottomup), 0), 32);
                RenderItem(GetItemForSlot(QuickBarPos + 1), new Location(center + (32 + 1), Window.Height - (32 + 16 + bottomup), 0), 32);
                RenderItem(GetItemForSlot(QuickBarPos + 2), new Location(center + (32 + 32 + 2), Window.Height - (32 + 16 + bottomup), 0), 32);
                RenderItem(GetItemForSlot(QuickBarPos), new Location(center - (32 + 1), Window.Height - (64 + bottomup), 0), 64);
                string it = "^%^e^7" + GetItemForSlot(QuickBarPos).DisplayName;
                float size = FontSets.Standard.MeasureFancyText(it);
                FontSets.Standard.DrawColoredText(it, new Location(center - size / 2f, Window.Height - (64 + bottomup) - FontSets.Standard.font_default.Height - 5, 0));
                float percent = 0;
                if (Player.MaxHealth != 0)
                {
                    percent = (float)Math.Round((Player.Health / Player.MaxHealth) * 10000) / 100f;
                }
                int healthbaroffset = 300;
                Textures.White.Bind();
                Rendering.SetColor(Color4.Black);
                Rendering.RenderRectangle(center - healthbaroffset, Window.Height - 30, center + healthbaroffset, Window.Height - 2);
                Rendering.SetColor(Color4.Red);
                Rendering.RenderRectangle(center - healthbaroffset + 2, Window.Height - 28, center - (healthbaroffset - 2) * ((100 - percent) / 100), Window.Height - 4);
                Rendering.SetColor(Color4.Cyan);
                Rendering.RenderRectangle(center + 2, Window.Height - 28, center + healthbaroffset - 2, Window.Height - 4); // TODO: Armor percent
                FontSets.SlightlyBigger.DrawColoredText("^S^!^e^0Health: " + Player.Health.ToString(healthformat) + "/" + Player.MaxHealth.ToString(healthformat) + " = " + percent.ToString(healthformat) + "%",
                    new Location(center - healthbaroffset + 4, Window.Height - 26, 0));
                FontSets.SlightlyBigger.DrawColoredText("^S^%^e^0Armor: " + "100.0" + "/" + "100.0" + " = " + "100.0" + "%", // TODO: Armor values!
                    new Location(center + 4, Window.Height - 26, 0));
                if (CVars.u_showmap.ValueB)
                {
                    Textures.White.Bind();
                    Rendering.SetColor(Color4.Black);
                    Rendering.RenderRectangle(Window.Width - 16 - 200, 16, Window.Width - 16, 16 + 200); // TODO: Dynamic size?
                    Rendering.SetColor(Color4.White);
                    GL.BindTexture(TextureTarget.Texture2D, map_fbo_texture);
                    Rendering.RenderRectangle(Window.Width - 16 - (200 - 2), 16 + 2, Window.Width - 16 - 2, 16 + (200 - 2));
                }
                int cX = Window.Width / 2;
                int cY = Window.Height / 2;
                int move = (int)Player.GetVelocity().LengthSquared() / 5;
                if (move > 20)
                {
                    move = 20;
                }
                Rendering.SetColor(Color4.White);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_tl").Bind(); // TODO: Save! Don't re-grab every tick!
                Rendering.RenderRectangle(cX - CVars.u_reticlescale.ValueI - move, cY - CVars.u_reticlescale.ValueI - move, cX - move, cY - move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_tr").Bind();
                Rendering.RenderRectangle(cX + move, cY - CVars.u_reticlescale.ValueI - move, cX + CVars.u_reticlescale.ValueI + move, cY - move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_bl").Bind();
                Rendering.RenderRectangle(cX - CVars.u_reticlescale.ValueI - move, cY + move, cX - move, cY + CVars.u_reticlescale.ValueI + move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_br").Bind();
                Rendering.RenderRectangle(cX + move, cY + move, cX + CVars.u_reticlescale.ValueI + move, cY + CVars.u_reticlescale.ValueI + move);
                if (CVars.u_showrangefinder.ValueB)
                {
                    FontSets.Standard.DrawColoredText(CameraDistance.ToString("#.0"), new Location(cX + move + CVars.u_reticlescale.ValueI, cY + move + CVars.u_reticlescale.ValueI, 0));
                }
            }
            RenderInvMenu();
        }

        /// <summary>
        /// Renders an item on the 2D screen.
        /// </summary>
        /// <param name="item">The item to render.</param>
        /// <param name="pos">Where to render it.</param>
        /// <param name="size">How big to render it, in pixels.</param>
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
