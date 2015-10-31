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

namespace Voxalia.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        public double gDelta = 0;

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
            s_shadow = Shaders.GetShader("shadow");
            s_main = Shaders.GetShader("test");
            s_finalgodray = Shaders.GetShader("finalgodray");
            s_fbo = Shaders.GetShader("fbo");
            s_fbov = Shaders.GetShader("fbo_vox");
            s_shadowadder = Shaders.GetShader("shadowadder");
            s_lightadder = Shaders.GetShader("lightadder");
            s_transponly = Shaders.GetShader("transponly");
            s_colormultvox = Shaders.GetShader("colormultvox");
            s_colormultr = Shaders.GetShader("color_multr");
            s_transponlyvox = Shaders.GetShader("transponlyvox");
            generateLightHelpers();
            ambient = new Location(0.1f);
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

        VBO[] skybox;

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
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Width, Window.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
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
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Window.Width, Window.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo2_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo2_texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public Shader s_shadow;
        public Shader s_main;
        public Shader s_finalgodray;
        public Shader s_fbo;
        public Shader s_fbov;
        public Shader s_shadowadder;
        public Shader s_lightadder;
        public Shader s_transponly;
        public Shader s_colormultvox;
        public Shader s_colormultr;
        public Shader s_transponlyvox;
        RenderSurface4Part RS4P;

        public Location CameraUp = Location.UnitZ;

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
                    CScreen.Render();
                    UIConsole.Draw();
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Rendering: " + ex.ToString());
                }
                try
                {
                    tick(e.Time);
                }
                catch (Exception ex)
                {
                    SysConsole.Output(OutputType.ERROR, "Renderticking: " + ex.ToString());
                }
                Window.SwapBuffers();
            }
        }

        public void renderGame()
        {
            RenderTextures = true;
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 1f, 1f, 1f });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
            GL.Enable(EnableCap.DepthTest);
            if (CVars.g_firstperson.ValueB)
            {
                CameraPos = PlayerEyePosition;
            }
            else
            {
                CameraPos = PlayerEyePosition - Player.ForwardVector() * 2;
            }
            sortEntities();
            if (CVars.r_lighting.ValueB)
            {
                SetViewport();
                CameraTarget = CameraPos + Player.ForwardVector();
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CVars.r_fov.ValueF), (float)Window.Width / (float)Window.Height, CVars.r_znear.ValueF, CVars.r_zfar.ValueF);
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(CameraPos), ClientUtilities.Convert(CameraTarget), ClientUtilities.Convert(CameraUp));
                Matrix4 combined = view * proj;
                Frustum camFrust = new Frustum(combined);
                if (shouldRedrawShadows && CVars.r_shadows.ValueB)
                {
                    shouldRedrawShadows = false;
                    s_shadow.Bind();
                    VBO.BonesIdentity();
                    RenderingShadows = true;
                    LightsC = 0;
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                        {
                            // TODO: If movement_near_light
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
                                    Lights[i].InternalLights[x].Attach();
                                    // TODO: Render settings
                                    Render3D(true);
                                    Lights[i].InternalLights[x].Complete();
                                }
                            }
                        }
                    }
                }
                SetViewport();
                s_fbov.Bind();
                GL.UniformMatrix4(1, false, ref combined);
                s_fbo.Bind();
                FBOid = 1;
                RenderingShadows = false;
                CFrust = camFrust;
                GL.UniformMatrix4(1, false, ref combined);
                GL.ActiveTexture(TextureUnit.Texture0);
                RS4P.Bind();
                RenderLights = true;
                // TODO: Render settings
                Render3D(false);
                RenderLights = false;
                RS4P.Unbind();
                FBOid = 0;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo2_main);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                if (CVars.r_shadows.ValueB)
                {
                    s_shadowadder.Bind();
                }
                else
                {
                    s_lightadder.Bind();
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
                Matrix4 mat = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);
                GL.UniformMatrix4(1, false, ref mat);
                mat = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref mat);
                GL.Uniform3(10, ClientUtilities.Convert(CameraPos));
                GL.Uniform1(13, CVars.r_shadowblur.ValueF);
                bool first = true;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo2_main);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0, 0, 0, 1 });
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.Disable(EnableCap.CullFace);
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos, Lights[i].MaxDistance))
                    {
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
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, first ? fbo_main : fbo2_main);
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, first ? fbo2_texture : fbo_texture);
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
                                    GL.Uniform1(9, 0f);
                                }
                                else
                                {
                                    GL.Uniform1(9, Lights[i].InternalLights[x].maxrange);
                                }
                                GL.Uniform1(12, 1f / Lights[i].InternalLights[x].texsize);
                                Rendering.RenderRectangle(-1, -1, 1, 1);
                                first = !first;
                                GL.ActiveTexture(TextureUnit.Texture0);
                                GL.BindTexture(TextureTarget.Texture2D, 0);
                            }
                        }
                    }
                }
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.DrawBuffer(DrawBufferMode.Back);
                if (CVars.r_godrays.ValueB)
                {
                    s_finalgodray.Bind();
                }
                else
                {
                    s_main.Bind();
                }
                GL.Uniform3(5, ClientUtilities.Convert(ambient));
                GL.Uniform3(8, ClientUtilities.Convert(CameraFinalTarget));
                GL.Uniform1(9, CVars.r_dof_strength.ValueF);
                Vector3 lPos = GetSunLocation();
                Vector4 t = Vector4.Transform(new Vector4(lPos, 1f), combined);
                Vector2 lightPos = (t.Xy / t.W) / 2f + new Vector2(0.5f);
                GL.Uniform2(10, ref lightPos);
                GL.Uniform1(11, CVars.r_godray_samples.ValueI);
                GL.Uniform1(12, CVars.r_godray_wexposure.ValueF);
                GL.Uniform1(13, CVars.r_godray_decay.ValueF);
                GL.Uniform1(14, CVars.r_godray_density.ValueF);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.bwtexture);
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
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
                GL.BlitFramebuffer(0, 0, Window.Width, Window.Height, 0, 0, Window.Width, Window.Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                Matrix4 def = Matrix4.Identity;
                s_colormultr.Bind();
                VBO.BonesIdentity();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                GL.Enable(EnableCap.CullFace);
                if (CVars.r_renderwireframe.ValueB)
                {
                    Render3DWires();
                }
                RenderSkyflare(combined);
                ReverseEntitiesOrder();
                Particles.Sort();
                s_transponlyvox.Bind();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                s_transponly.Bind();
                VBO.BonesIdentity();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                FBOid = 3;
                //TranspBlend();
                GL.DepthMask(false);
                Render3D(false);
                FBOid = 0;
                RenderSkyflare(combined);
                //StandardBlend();
                GL.DepthMask(true);
            }
            else
            {
                SetViewport();
                Location CameraTarget = CameraPos + Player.ForwardVector();
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(CVars.r_fov.ValueF), (float)Window.Width / (float)Window.Height, CVars.r_znear.ValueF, CVars.r_zfar.ValueF);
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(CameraPos), ClientUtilities.Convert(CameraTarget), ClientUtilities.Convert(CameraUp));
                Matrix4 combined = view * proj;
                GL.ActiveTexture(TextureUnit.Texture0);
                ReverseEntitiesOrder();
                s_colormultvox.Bind();
                RenderLights = false;
                RenderTextures = true;
                RenderingShadows = false;
                GL.UniformMatrix4(1, false, ref combined);
                Matrix4 def = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref def);
                VBO.BonesIdentity();
                s_colormultr.Bind();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                VBO.BonesIdentity();
                CFrust = new Frustum(combined);
                FBOid = 2;
                Particles.Sort();
                Render3D(false);
                FBOid = 0;
                if (CVars.r_renderwireframe.ValueB)
                {
                    Render3DWires();
                }
                RenderSkyflare(combined);
                ReverseEntitiesOrder();
                s_transponlyvox.Bind();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                s_transponly.Bind();
                VBO.BonesIdentity();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
                FBOid = 3;
                Render3D(false);
                FBOid = 0;
                RenderSkyflare(combined);
            }
            Establish2D();
            Render2D();
        }

        float dist = 340; // TODO: View rad

        public Vector3 GetSunLocation()
        {
            return ClientUtilities.Convert(CameraPos + TheSun.Direction * -(dist * 0.96f));
        }

        public void RenderSkybox()
        {
            if (FBOid == 1)
            {
                GL.Uniform4(0, new Vector4(0f, 0f, 0f, 0f));
            }
            Rendering.SetMinimumLight(1);
            GL.Disable(EnableCap.CullFace);
            Matrix4 scale = Matrix4.CreateScale(dist, dist, dist) * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos));
            GL.UniformMatrix4(2, false, ref scale);
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
                GL.Uniform4(0, Color4.White);
            }
            Textures.GetTexture("skies/sun").Bind(); // TODO: Store var? Make dynamic?
            Matrix4 rot = Matrix4.CreateTranslation(-50f, -50f, 0f)
                * Matrix4.CreateRotationY((float)((-SunAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + SunAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos + TheSun.Direction * -(dist * 0.96f))); // TODO: adjust based on view rad
            Rendering.RenderRectangle(0, 0, 100, 100, rot); // TODO: Adjust scale based on view rad
            if (FBOid == 1)
            {
                GL.Uniform4(0, Color4.Black);
            }
            Textures.GetTexture("skies/planet").Bind(); // TODO: Store var? Make dynamic?
            Rendering.SetColor(new Color4(PlanetLight, PlanetLight, PlanetLight, 1));
            rot = Matrix4.CreateTranslation(-150f, -150f, 0f)
                * Matrix4.CreateRotationY((float)((-PlanetAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + PlanetAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(CameraPos + ThePlanet.Direction * -(dist * 0.8f))); // TODO: adjust based on view rad
            Rendering.RenderRectangle(0, 0, 300, 300, rot); // TODO: Adjust scale based on view rad
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.CullFace);
            Matrix4 ident = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref ident);
            Rendering.SetMinimumLight(0);
        }

        public void RenderSkyflare(Matrix4 combined)
        {
            /*
            if (CVars.r_lensflare.ValueB)
            {
                if (PlanetSunDist > 21f)
                {
                    Location rel = CameraPos + TheSun.Direction * -200f;
                    Vector4 vec = new Vector4((float)rel.X, (float)rel.Y, (float)rel.Z, 1f);
                    vec = Vector4.Transform(vec, combined);
                    float dist = 200; // TODO: View rad * 0.75 or something
                    Location sunpos = new Location(vec.X / vec.W, vec.Y / vec.W, vec.Z / vec.W);
                    if (sunpos.X >= -1 && sunpos.X <= 1 && sunpos.Y >= -1 && sunpos.Y <= 1 && sunpos.Z <= 1 && sunpos.Z >= -1)
                    {
                        CollisionResult trace = TheRegion.Collision.RayTrace(CameraPos, CameraPos + TheSun.Direction * -dist, Player.IgnoreThis);
                        if (!trace.Hit)
                        {
                            Rendering.SetColor(Color4.Yellow);
                            Matrix4 ident = Matrix4.Identity;
                            GL.UniformMatrix4(1, false, ref ident);
                            GL.UniformMatrix4(2, false, ref ident);
                            Location start = new Location(sunpos.X, sunpos.Y, 0);
                            Location targ = new Location(0, 0, 0);
                            GL.Disable(EnableCap.CullFace);
                            int c = 6;
                            Location move = (targ - start) / (c / 2);
                            for (int i = 0; i < c; i++)
                            {
                                Textures.GetTexture("effects/lensflare/0" + (i + 1)).Bind(); // TODO: Store better
                                Location fs = start + move * (i + 1);
                                float s = 0.2f - ((float)i) * 0.025f;
                                Rendering.RenderRectangle((float)fs.X - s, (float)fs.Y - s, (float)fs.X + s, (float)fs.Y + s, Matrix4.CreateTranslation(0, 0, 0.1f * c - 0.1f * i));
                            }
                            GL.BindTexture(TextureTarget.Texture2D, 0);
                            Rendering.SetColor(Color4.White);
                            GL.Enable(EnableCap.CullFace);
                        }
                    }
                }
            }
            */
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
                GL.Uniform4(0, Color4.Black);
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
                if (FBOid == 1)
                {
                    Rendering.SetSpecular(0);
                    Rendering.SetMinimumLight(1);
                }
                RenderSkybox();
                if (FBOid == 1)
                {
                    Rendering.SetSpecular(1);
                    Rendering.SetMinimumLight(0);
                }
                for (int i = 0; i < TheRegion.Entities.Count; i++)
                {
                    TheRegion.Entities[i].Render();
                }
                if (FBOid == 1)
                {
                    Rendering.SetMinimumLight(1f);
                    Rendering.SetSpecular(0);
                }
                Particles.Engine.Render();
            }
            if (FBOid == 1)
            {
                s_fbov.Bind();
                if (FBOid == 1)
                {
                    GL.Uniform4(0, Color4.Black);
                }
            }
            else if (FBOid == 2)
            {
                s_colormultvox.Bind();
            }
            else if (FBOid == 3)
            {
                s_transponlyvox.Bind();
            }
            TheRegion.Render();
            if (FBOid == 1)
            {
                s_fbo.Bind();
                if (FBOid == 1)
                {
                    GL.Uniform4(0, Color4.Black);
                }
            }
            else if (FBOid == 2)
            {
                s_colormultr.Bind();
            }
            else if (FBOid == 3)
            {
                s_transponly.Bind();
            }
            Textures.White.Bind();
            Location mov = (CameraFinalTarget - PlayerEyePosition) / CameraDistance;
            Location cpos = CameraFinalTarget - (CameraImpactNormal * 0.01f);
            Location cpos2 = CameraFinalTarget + (CameraImpactNormal * 0.91f);
            // TODO: 5 -> Variable length (Server controlled?)
            if (TheRegion.GetBlockMaterial(cpos) != Material.AIR && CameraDistance < 5)
            {
                if (CVars.r_highlight_targetblock.ValueB)
                {
                    Location cft = cpos.GetBlockLocation();
                    GL.LineWidth(3);
                    Rendering.SetColor(Color4.Blue);
                    Rendering.SetMinimumLight(1.0f);
                    Rendering.RenderLineBox(cft - mov * 0.01f, cft + Location.One - mov * 0.01f);
                    GL.LineWidth(1);
                }
                if (CVars.r_highlight_placeblock.ValueB)
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
            for (int i = 0; i < TheRegion.Joints.Count; i++)
            {
                // TODO: Only render if set to
                if (TheRegion.Joints[i] is JointDistance)
                {
                    Rendering.RenderLine(((JointDistance)TheRegion.Joints[i]).Ent1Pos + TheRegion.Joints[i].One.GetPosition(), ((JointDistance)TheRegion.Joints[i]).Ent2Pos + TheRegion.Joints[i].Two.GetPosition());
                }
                else
                {
                    //Rendering.RenderLine(Joints[i].Ent1.GetPosition(), Joints[i].Ent2.GetPosition());
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
            for (int i = 0; i < TheRegion.Entities.Count; i++)
            {
                Rendering.SetColor(TheRegion.Entities[i].Color);
                TheRegion.Entities[i].Render();
            }
            TheRegion.Render();
            // TODO: Render joints?
            GL.Enable(EnableCap.DepthTest);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.LineWidth(1);
            Rendering.SetColor(Color4.White);
            RenderTextures = rt;
        }

        public double RenderExtraItems = 0;

        public void Render2D()
        {
            GL.Disable(EnableCap.CullFace);
            if (CVars.u_showhud.ValueB)
            {
                FontSets.Standard.DrawColoredText("^!^e^7gFPS(calc): " + (1f / gDelta) + ", gFPS(actual): " + gFPS
                    + "\n" + Player.GetPosition()
                    + "\n" + Player.GetVelocity() + " == " + Player.GetVelocity().Length()
                    + "\nLight source(s): " + LightsC
                    + "\nEntities: " + TheRegion.Entities.Count
                    + "\nFLAGS: " + Player.ServerFlags, new Location(0, 0, 0));
                int center = Window.Width / 2;
                if (RenderExtraItems > 0)
                {
                    RenderExtraItems -= gDelta;
                    if (RenderExtraItems < 0)
                    {
                        RenderExtraItems = 0;
                    }
                    RenderItem(GetItemForSlot(QuickBarPos - 5), new Location(center - (32 + 32 + 32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos - 4), new Location(center - (32 + 32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos - 3), new Location(center - (32 + 32 + 32 + 32 + 3), Window.Height - (32 + 16), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 3), new Location(center + (32 + 32 + 32 + 2), Window.Height - (32 + 16), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 4), new Location(center + (32 + 32 + 32 + 32 + 2), Window.Height - (32 + 16), 0), 32);
                    RenderItem(GetItemForSlot(QuickBarPos + 5), new Location(center + (32 + 32 + 32 + 32 + 32 + 2), Window.Height - (32 + 16), 0), 32);
                }
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
                int cX = Window.Width / 2;
                int cY = Window.Height / 2;
                int move = (int)Player.GetVelocity().LengthSquared() / 5;
                if (move > 20)
                {
                    move = 20;
                }
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_tl").Bind();
                Rendering.RenderRectangle(cX - CVars.u_reticlescale.ValueI - move, cY - CVars.u_reticlescale.ValueI - move, cX - move, cY - move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_tr").Bind();
                Rendering.RenderRectangle(cX + move, cY - CVars.u_reticlescale.ValueI - move, cX + CVars.u_reticlescale.ValueI + move, cY - move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_bl").Bind();
                Rendering.RenderRectangle(cX - CVars.u_reticlescale.ValueI - move, cY + move, cX - move, cY + CVars.u_reticlescale.ValueI + move);
                Textures.GetTexture("ui/hud/reticles/" + CVars.u_reticle.Value + "_br").Bind();
                Rendering.RenderRectangle(cX + move, cY + move, cX + CVars.u_reticlescale.ValueI + move, cY + CVars.u_reticlescale.ValueI + move);
            }
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
                if (size >= 64 && item.Datum != 0)
                {
                    string dat = "^!^e^7^S" + item.Datum;
                    FontSets.SlightlyBigger.DrawColoredText(dat, new Location(pos.X + size - FontSets.SlightlyBigger.MeasureFancyText(dat), pos.Y + size - FontSets.SlightlyBigger.font_default.Height / 2f - 5, 0));
                }
            }
        }

        public int vpw = 800;
        public int vph = 600;

        public bool RenderLights = false;

        public Matrix4 Ortho;
    }
}
