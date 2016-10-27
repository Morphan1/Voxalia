//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

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

        public Stack<VBO> vbos = new Stack<VBO>(200);

        public Stack<ChunkRenderHelper> RenderHelpers = new Stack<ChunkRenderHelper>(200);

        public List<Tuple<string, long>> CalculateVRAMUsage()
        {
            List<Tuple<string, long>> toret = new List<Tuple<string, long>>();
            long modelc = 0;
            foreach (Model model in Models.LoadedModels)
            {
                modelc += model.GetVRAMUsage();
            }
            toret.Add(new Tuple<string, long>("Models", modelc));
            long texturec = 0;
            foreach (Texture texture in Textures.LoadedTextures)
            {
                texturec += texture.Width * texture.Height * 4;
            }
            toret.Add(new Tuple<string, long>("Textures", texturec));
            long blocktexturec = TBlock.TWidth * TBlock.TWidth * 4 * 3;
            for (int i = 0; i < TBlock.Anims.Count; i++)
            {
                blocktexturec += TBlock.TWidth * TBlock.TWidth * 4 * TBlock.Anims[i].FBOs.Length;
            }
            toret.Add(new Tuple<string, long>("BlockTextures", blocktexturec));
            long chunkc = 0;
            foreach (Chunk chunk in TheRegion.LoadedChunks.Values)
            {
                if (chunk._VBO != null)
                {
                    chunkc += chunk._VBO.GetVRAMUsage();
                }
            }
            toret.Add(new Tuple<string, long>("chunks", chunkc));
            // TODO: Maybe also View3D render helper usage?
            return toret;
        }

        void PreInitRendering()
        {
            GL.Viewport(0, 0, Window.Width, Window.Height);
            GL.Enable(EnableCap.Texture2D); // TODO: Other texture modes we use as well?
            GL.Enable(EnableCap.Blend);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
        }

        public View3D MainWorldView = new View3D();
        
        void InitRendering()
        {
            MainWorldView.CameraModifier = () => Player.GetRelativeQuaternion();
            ShadersCheck();
            View3D.CheckError("Load - Rendering - Shaders");
            generateMapHelpers();
            View3D.CheckError("Load - Rendering - Map");
            MainWorldView.ShadowingAllowed = true;
            MainWorldView.ShadowTexSize = () => CVars.r_shadowquality.ValueI;
            MainWorldView.Render3D = Render3D;
            MainWorldView.PostFirstRender = ReverseEntitiesOrder;
            MainWorldView.LLActive = CVars.r_transpll.ValueB; // TODO: CVar edit call back
            View3D.CheckError("Load - Rendering - Settings");
            MainWorldView.Generate(this, Window.Width, Window.Height);
            View3D.CheckError("Load - Rendering - ViewGen");
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
            View3D.CheckError("Load - Rendering - VBO Prep");
            for (int i = 0; i < 6; i++)
            {
                skybox[i].GenerateVBO();
            }
            View3D.CheckError("Load - Rendering - Final");
        }

        public void ShadersCheck()
        {
            string def = CVars.r_good_graphics.ValueB ? "#MCM_GOOD_GRAPHICS" : "#";
            s_shadow = Shaders.GetShader("shadow" + def);
            s_shadowvox = Shaders.GetShader("shadowvox" + def);
            s_fbo = Shaders.GetShader("fbo" + def);
            s_fbot = Shaders.GetShader("fbo" + def + ",MCM_TRANSP_ALLOWED");
            s_fbov = Shaders.GetShader("fbo_vox" + def);
            s_fbo_refract = Shaders.GetShader("fbo" + def + ",MCM_REFRACT");
            s_fbov_refract = Shaders.GetShader("fbo_vox" + def + ",MCM_REFRACT");
            s_shadowadder = Shaders.GetShader("lightadder" + def + ",MCM_SHADOWS");
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
            s_finalgodray_lights = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS");
            s_finalgodray_lights_toonify = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_TOONIFY");
            s_finalgodray_lights_motblur = Shaders.GetShader("finalgodray" + def + ",MCM_LIGHTS,MCM_MOTBLUR");
            s_forw = Shaders.GetShader("forward" + def);
            s_forw_vox = Shaders.GetShader("forward" + def + ",MCM_VOX");
            s_forw_trans = Shaders.GetShader("forward" + def + ",MCM_TRANSP");
            s_forw_vox_trans = Shaders.GetShader("forward" + def + ",MCM_VOX,MCM_TRANSP");
            s_transponly_ll = Shaders.GetShader("transponly" + def + ",MCM_LL");
            s_transponlyvox_ll = Shaders.GetShader("transponlyvox" + def + ",MCM_LL");
            s_transponlylit_ll = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_LL");
            s_transponlyvoxlit_ll = Shaders.GetShader("transponlyvox" + def + ",MCM_LIT,MCM_LL");
            s_transponlylitsh_ll = Shaders.GetShader("transponly" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL");
            s_transponlyvoxlitsh_ll = Shaders.GetShader("transponlyvox" + def + ",MCM_LIT,MCM_SHADOWS,MCM_LL");
            s_ll_clearer = Shaders.GetShader("clearer" + def);
            s_ll_fpass = Shaders.GetShader("fpass" + def);
            s_hdrpass = Shaders.GetShader("hdrpass" + def);
            s_forw_grass = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE?grass");
            s_fbo_grass = Shaders.GetShader("fbo" + def + ",MCM_GEOM_ACTIVE,MCM_PRETTY?grass");
            s_forw_particles = Shaders.GetShader("forward" + def + ",MCM_GEOM_ACTIVE,MCM_TRANSP,MCM_NO_ALPHA_CAP?particles");
            // TODO: Better place for models?
            RainCyl = Models.GetModel("raincyl");
            RainCyl.LoadSkin(Textures);
            SnowCyl = Models.GetModel("snowcyl");
            SnowCyl.LoadSkin(Textures);
        }

        public Model RainCyl;

        public Model SnowCyl;

        int map_fbo_main = -1;
        int map_fbo_texture = -1;
        int map_fbo_depthtex = -1;

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
        
        VBO[] skybox;

        public Shader s_shadow;
        public Shader s_finalgodray;
        public Shader s_finalgodray_lights;
        public Shader s_finalgodray_toonify;
        public Shader s_finalgodray_lights_toonify;
        public Shader s_finalgodray_lights_motblur;
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
        public Shader s_forw;
        public Shader s_forw_vox;
        public Shader s_forw_trans;
        public Shader s_forw_vox_trans;
        public Shader s_transponly_ll;
        public Shader s_transponlyvox_ll;
        public Shader s_transponlylit_ll;
        public Shader s_transponlyvoxlit_ll;
        public Shader s_transponlylitsh_ll;
        public Shader s_transponlyvoxlitsh_ll;
        public Shader s_ll_clearer;
        public Shader s_ll_fpass;
        public Shader s_hdrpass;
        public Shader s_forw_grass;
        public Shader s_fbo_grass;
        public Shader s_forw_particles;
        
        public void sortEntities()
        {
            TheRegion.Entities = TheRegion.Entities.OrderBy(o => (o.GetPosition().DistanceSquared(MainWorldView.RenderRelative))).ToList();
        }

        public void ReverseEntitiesOrder()
        {
            TheRegion.Entities.Reverse();
        }

        public int gTicks = 0;

        public int gFPS = 0;
        
        int rTicks = 1000;

        public bool shouldRedrawShadows = false;
        
        public void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            lock (TickLock)
            {
                gDelta = e.Time;
                gTicks++;
                if (Window.Visible && Window.WindowState != WindowState.Minimized)
                {
                    try
                    {
                        Shaders.ColorMultShader.Bind();
                        GL.Uniform1(6, (float)GlobalTickTimeLocal);
                        if (CVars.r_3d_enable.ValueB)
                        {
                            GL.Viewport(Window.Width / 2, 0, Window.Width / 2, Window.Height);
                            Render2D(false);
                            UIConsole.Draw();
                            GL.Viewport(0, 0, Window.Width / 2, Window.Height);
                            Render2D(false);
                            UIConsole.Draw();
                            GL.Viewport(0, 0, Window.Width, Window.Height);
                        }
                        else
                        {
                            Render2D(false);
                            UIConsole.Draw();
                        }
                    }
                    catch (Exception ex)
                    {
                        SysConsole.Output(OutputType.ERROR, "Rendering (general): " + ex.ToString());
                    }
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
                View3D.CheckError("Finish");
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

        public double TickTime;
        public double FinishTime;
        public double TWODTime;
        public double TotalTime;
        public double TickSpikeTime;
        public double FinishSpikeTime;
        public double TWODSpikeTime;
        public double TotalSpikeTime;

        public double mapLastRendered = 0;

        public void renderGame()
        {
            Stopwatch totalt = new Stopwatch();
            totalt.Start();
            try
            {
                MainWorldView.ForwardVec = Player.ForwardVector();
                // Frustum cf1 = null;
                if (CVars.g_firstperson.ValueB)
                {
                    MainWorldView.CameraPos = PlayerEyePosition;
                }
                else
                {
                    CollisionResult cr = TheRegion.Collision.RayTrace(PlayerEyePosition, PlayerEyePosition - MainWorldView.CalcForward() * Player.ViewBackMod(), IgnorePlayer);
                    if (cr.Hit)
                    {
                        MainWorldView.CameraPos = cr.Position + cr.Normal * 0.05;
                    }
                    else
                    {
                        MainWorldView.CameraPos = cr.Position;
                    }
                }
                if (CVars.u_showmap.ValueB && mapLastRendered + 1.0 < TheRegion.GlobalTickTimeLocal) // TODO: 1.0 -> custom
                {
                    mapLastRendered = TheRegion.GlobalTickTimeLocal;
                    AABB box = new AABB() { Min = Player.GetPosition(), Max = Player.GetPosition() };
                    foreach (Chunk ch in TheRegion.LoadedChunks.Values)
                    {
                        box.Include(ch.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE);
                        box.Include(ch.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE + new Location(Chunk.CHUNK_SIZE));
                    }
                    Matrix4 ortho = Matrix4.CreateOrthographicOffCenter((float)box.Min.X, (float)box.Max.X, (float)box.Min.Y, (float)box.Max.Y, (float)box.Min.Z, (float)box.Max.Z);
                    //  Matrix4 oident = Matrix4.Identity;
                    s_mapvox = s_mapvox.Bind();
                    GL.UniformMatrix4(View3D.MAT_LOC_VIEW, false, ref ortho);
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
                Particles.Sort();
                MainWorldView.Headmat = TheRegion.GetBlockMaterial(PlayerEyePosition);
                MainWorldView.SunLoc = GetSunLocation();
                MainWorldView.Render();
                ReverseEntitiesOrder();
            }
            catch (Exception ex)
            {
                SysConsole.Output("Rendering (2D)", ex);
            }
            try
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                Establish2D();
                if (CVars.r_3d_enable.ValueB)
                {
                    GL.Viewport(Window.Width / 2, 0, Window.Width / 2, Window.Height);
                    //Render2D(false);
                    GL.Viewport(0, 0, Window.Width / 2, Window.Height);
                    //Render2D(false);
                    GL.Viewport(0, 0, Window.Width, Window.Height);
                }
                else
                {
                    //Render2D(false);
                }
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
            totalt.Stop();
            TotalTime = (double)totalt.ElapsedMilliseconds / 1000f;
            if (TotalTime > TotalSpikeTime)
            {
                TotalSpikeTime = TotalTime;
            }
        }

        float dist2 = 1900; // TODO: (View rad + 2) * CHUNK_SIZE ? Or base off ZFAR?
        public float dist = 1700;
        
        public Location GetSunLocation()
        {
            return MainWorldView.CameraPos + TheSun.Direction * -(dist * 0.96f);
        }

        public void RenderSkybox()
        {
            Rendering.SetMinimumLight(1);
            GL.Disable(EnableCap.CullFace);
            Rendering.SetColor(Color4.White);
            Matrix4 scale = Matrix4.CreateScale(dist2, dist2, dist2);
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
            Rendering.SetColor(new Vector4(1, 1, 1, (float)Math.Max(Math.Min((SunAngle.Pitch - 70.0) / (-90.0), 1.0), 0.06)));
            scale = Matrix4.CreateScale(dist, dist, dist);
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
            Rendering.SetColor(new Vector4(ClientUtilities.Convert(Location.One * SunLightModDirect), 1));
            Textures.GetTexture("skies/sun").Bind(); // TODO: Store var!
            Matrix4 rot = Matrix4.CreateTranslation(-150f, -150f, 0f)
                * Matrix4.CreateRotationY((float)((-SunAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + SunAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(TheSun.Direction * -(dist * 0.96f)));
            Rendering.RenderRectangle(0, 0, 300, 300, rot); // TODO: Adjust scale based on view rad
            Textures.GetTexture("skies/planet").Bind(); // TODO: Store var!
            Rendering.SetColor(new Color4(PlanetLight, PlanetLight, PlanetLight, 1));
            rot = Matrix4.CreateTranslation(-450f, -450f, 0f)
                * Matrix4.CreateRotationY((float)((-PlanetAngle.Pitch - 90f) * Utilities.PI180))
                * Matrix4.CreateRotationZ((float)((180f + PlanetAngle.Yaw) * Utilities.PI180))
                * Matrix4.CreateTranslation(ClientUtilities.Convert(PlanetDir * -(dist * 0.8f)));
            Rendering.RenderRectangle(0, 0, 900, 900, rot);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.CullFace);
            Matrix4 ident = Matrix4.Identity;
            GL.UniformMatrix4(2, false, ref ident);
            Rendering.SetColor(Color4.White);
            Rendering.SetMinimumLight(0);
        }
        
        public void Establish2D()
        {
            GL.Disable(EnableCap.DepthTest);
            Shaders.ColorMultShader.Bind();
            Ortho = Matrix4.CreateOrthographicOffCenter(0, Window.Width, Window.Height, 0, -1, 1);
            GL.UniformMatrix4(1, false, ref Ortho);
        }

        public bool isVox = false;

        public void SetVox()
        {
            if (isVox)
            {
                return;
            }
            isVox = true;
            if (MainWorldView.FBOid == FBOID.MAIN)
            {
                s_fbov = s_fbov.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            if (MainWorldView.FBOid == FBOID.REFRACT)
            {
                s_fbov_refract = s_fbov_refract.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_UNLIT)
            {
                s_transponlyvox = s_transponlyvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LIT)
            {
                s_transponlyvoxlit = s_transponlyvoxlit.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_SHADOWS)
            {
                s_transponlyvoxlitsh = s_transponlyvoxlitsh.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LL)
            {
                s_transponlyvox_ll = s_transponlyvox_ll.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LIT_LL)
            {
                s_transponlyvoxlit_ll = s_transponlyvoxlit_ll.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_SHADOWS_LL)
            {
                s_transponlyvoxlitsh_ll = s_transponlyvoxlitsh_ll.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.NormalTextureID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.HelpTextureID);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            else if (MainWorldView.FBOid == FBOID.FORWARD_SOLID)
            {
                s_forw_vox = s_forw_vox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
            }
            else if (MainWorldView.FBOid == FBOID.FORWARD_TRANSP)
            {
                s_forw_vox_trans = s_forw_vox_trans.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
            }
            else if (MainWorldView.FBOid == FBOID.SHADOWS)
            {
                s_shadowvox = s_shadowvox.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, TBlock.TextureID);
            }
            if (FixPersp != Matrix4.Identity)
            {
                GL.UniformMatrix4(View3D.MAT_LOC_VIEW, false, ref FixPersp);
            }
        }

        public void SetEnts()
        {
            if (!isVox)
            {
                return;
            }
            isVox = false;
            if (MainWorldView.FBOid == FBOID.MAIN)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_fbo = s_fbo.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.REFRACT)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_fbo_refract = s_fbo_refract.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_UNLIT)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponly = s_transponly.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LIT)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponlylit = s_transponlylit.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_SHADOWS)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponlylitsh = s_transponlylitsh.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LL)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponly_ll = s_transponly_ll.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_LIT_LL)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponlylit_ll = s_transponlylit_ll.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.TRANSP_SHADOWS_LL)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                s_transponlylitsh_ll = s_transponlylitsh_ll.Bind();
            }
            else if (MainWorldView.FBOid == FBOID.FORWARD_SOLID)
            {
                s_forw = s_forw.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
            }
            else if (MainWorldView.FBOid == FBOID.FORWARD_TRANSP)
            {
                s_forw_trans = s_forw_trans.Bind();
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
            }
            else if (MainWorldView.FBOid == FBOID.SHADOWS)
            {
                GL.BindTexture(TextureTarget.Texture2DArray, 0);
                s_shadow = s_shadow.Bind();
            }
            if (FixPersp != Matrix4.Identity)
            {
                GL.UniformMatrix4(View3D.MAT_LOC_VIEW, false, ref FixPersp);
            }
        }

        public double RainCylPos = 0;

        public void Render3D(View3D view)
        {
            GL.Enable(EnableCap.CullFace);
            if (view.ShadowsOnly)
            {
                for (int i = 0; i < TheRegion.ShadowCasters.Count; i++)
                {
                    TheRegion.ShadowCasters[i].Render();
                }
                if (view.TranspShadows)
                {
                    TheRegion.RenderClouds();
                }
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                if (view.FBOid == FBOID.MAIN)
                {
                    s_fbot.Bind();
                    RenderSkybox();
                    s_fbo.Bind();
                }
                if (view.FBOid == FBOID.FORWARD_SOLID || view.FBOid == FBOID.FORWARD_TRANSP)
                {
                    RenderSkybox(); // TODO: s_fbot equivalent for forward renderer?
                }
                if (view.FBOid == FBOID.TRANSP_UNLIT || view.FBOid == FBOID.TRANSP_LIT || view.FBOid == FBOID.TRANSP_SHADOWS
                    || view.FBOid == FBOID.FORWARD_SOLID || view.FBOid == FBOID.FORWARD_TRANSP)
                {
                    Rendering.SetMinimumLight(1);
                    TheRegion.RenderClouds();
                    Rendering.SetMinimumLight(0);
                }
                for (int i = 0; i < TheRegion.Entities.Count; i++)
                {
                    TheRegion.Entities[i].Render();
                }
                SetEnts();
                if (CVars.g_weathermode.ValueI > 0)
                {
                    RainCylPos += gDelta * ((CVars.g_weathermode.ValueI == 1) ? 0.5 : 0.1);
                    while (RainCylPos > 1.0)
                    {
                        RainCylPos -= 1.0;
                    }
                    Matrix4d rot = (CVars.g_weathermode.ValueI == 2) ? Matrix4d.CreateRotationZ(Math.Sin(RainCylPos * 2f * Math.PI) * 0.1f) : Matrix4d.Identity;
                    for (int i = -10; i <= 10; i++)
                    {
                        Matrix4d mat = rot * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(MainWorldView.CameraPos + new Location(0, 0, 4 * i + RainCylPos * -4)));
                        MainWorldView.SetMatrix(2, mat);
                        if (CVars.g_weathermode.ValueI == 1)
                        {
                            RainCyl.Draw();
                        }
                        else if (CVars.g_weathermode.ValueI == 2)
                        {
                            SnowCyl.Draw();
                        }
                    }
                }
                if (MainWorldView.FBOid == FBOID.MAIN)
                {
                    Rendering.SetMinimumLight(1f);
                }
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                Particles.Engine.Render();
            }
            SetEnts();
            isVox = false;
            SetVox();
            TheRegion.Render();
            SetEnts();
            TheRegion.RenderPlants();
            if (!view.ShadowsOnly)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                Textures.NormalDef.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
            }
            Textures.White.Bind();
            Location mov = (CameraFinalTarget - MainWorldView.CameraPos) / CameraDistance;
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
            if (MainWorldView.FBOid == FBOID.MAIN)
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
                        Rendering.RenderLineBox(chunk.WorldPosition.ToLocation() * Chunk.CHUNK_SIZE, (chunk.WorldPosition.ToLocation() + Location.One) * Chunk.CHUNK_SIZE);
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
                                Vector4 col = Rendering.AdaptColor(ClientUtilities.ConvertD((one + two) * 0.5), ((ConnectorBeam)TheRegion.Joints[i]).color);
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
                                    BEPUutilities.Quaternion quat = BEPUutilities.Quaternion.CreateFromAxisAngle(forwvec, (float)(tang % (Math.PI * 2.0)));
                                    res = BEPUutilities.Quaternion.Transform(res, quat);
                                    res = res * (float)(0.1 * forlen);
                                    DrawCurve(one, two, spos + new Location(res), ((ConnectorBeam)TheRegion.Joints[i]).color);
                                }
                            }
                            break;
                    }
                }
            }
            Rendering.SetColor(Color4.White);
            Rendering.SetMinimumLight(0);
            Textures.White.Bind();
            if (!view.ShadowsOnly)
            {
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                Render2D(true);
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
                Rendering.RenderBilboardLine(curvePos, c2, 3, MainWorldView.CameraPos);
                curvePos = c2;
            }
        }

        Location CalculateBezierPoint(double t, Location p0, Location p1, Location p2)
        {
            double u = 1 - t;
            return (u * u) * p0 + 2 * u * t * p1 + t * t * p2;
        }

        public bool RenderTextures = true;
        
        public double RenderExtraItems = 0;

        const string timeformat = "#.000";

        const string healthformat = "0.0";

        const string pingformat = "000";

        public Matrix4 FixPersp = Matrix4.Identity;

        public int UIBottomHeight = (itemScale * 2 + bottomup) + itemScale * 2;

        const int itemScale = 48;

        const int bottomup = 32 + 32;

        public void Render2D(bool sub3d)
        {
            if (sub3d)
            {
                //GL.Disable(EnableCap.DepthTest);
                FixPersp = Matrix4.CreateOrthographicOffCenter(0, Window.Width, Window.Height, 0, -(itemScale * 2), 1000);
                isVox = false;
                SetVox();
            }
            GL.Disable(EnableCap.CullFace);
            if (CVars.u_showhud.ValueB && !InvShown())
            {
                if (!sub3d && CVars.u_showping.ValueB)
                {
                    string pingdetail = "^0^e^&ping: " + (Math.Max(LastPingValue, GlobalTickTimeLocal - LastPingTime) * 1000.0).ToString(pingformat) + "ms";
                    string pingdet2 = "^0^e^&average: " + (APing * 1000.0).ToString(pingformat) + "ms";
                    FontSets.Standard.DrawColoredText(pingdetail, new Location(Window.Width - FontSets.Standard.MeasureFancyText(pingdetail), Window.Height - FontSets.Standard.font_default.Height * 2, 0));
                    FontSets.Standard.DrawColoredText(pingdet2, new Location(Window.Width - FontSets.Standard.MeasureFancyText(pingdet2), Window.Height - FontSets.Standard.font_default.Height, 0));
                }
                if (!sub3d && CVars.u_debug.ValueB)
                {
                    FontSets.Standard.DrawColoredText(FontSets.Standard.SplitAppropriately("^!^e^7gFPS(calc): " + (1f / gDelta) + ", gFPS(actual): " + gFPS
                        + "\nHeld Item: " + GetItemForSlot(QuickBarPos).ToString()
                        + "\nTimes -> Phyiscs: " + TheRegion.PhysTime.ToString(timeformat) + ", Shadows: " + MainWorldView.ShadowTime.ToString(timeformat)
                        + ", FBO: " + MainWorldView.FBOTime.ToString(timeformat) + ", Lights: " + MainWorldView.LightsTime.ToString(timeformat) + ", 2D: " + TWODTime.ToString(timeformat)
                        + ", Tick: " + TickTime.ToString(timeformat) + ", Finish: " + FinishTime.ToString(timeformat) + ", Total: " + TotalTime.ToString(timeformat)
                        + "\nSpike Times -> Shadows: " + MainWorldView.ShadowSpikeTime.ToString(timeformat)
                        + ", FBO: " + MainWorldView.FBOSpikeTime.ToString(timeformat) + ", Lights: " + MainWorldView.LightsSpikeTime.ToString(timeformat) + ", 2D: " + TWODSpikeTime.ToString(timeformat)
                        + ", Tick: " + TickSpikeTime.ToString(timeformat) + ", Finish: " + FinishSpikeTime.ToString(timeformat) + ", Total: " + TotalSpikeTime.ToString(timeformat)
                        + "\nChunks loaded: " + TheRegion.LoadedChunks.Count + ", Chunks rendering currently: " + TheRegion.RenderingNow.Count + ", chunks waiting: " + TheRegion.NeedsRendering.Count + ", Entities loaded: " + TheRegion.Entities.Count
                        + "\nChunks prepping currently: " + TheRegion.PreppingNow.Count + ", chunks waiting for prep: " + TheRegion.PrepChunks.Count
                        + "\nPosition: " + Player.GetPosition().ToBasicString() + ", velocity: " + Player.GetVelocity().ToBasicString() + ", direction: " + Player.Direction.ToBasicString()
                        + "\nExposure: " + MainWorldView.MainEXP,
                        Window.Width - 10), new Location(0, 0, 0), Window.Height, 1, false, "^r^!^e^7");
                }
                int center = Window.Width / 2;
                if (RenderExtraItems > 0)
                {
                    RenderExtraItems -= gDelta;
                    if (RenderExtraItems < 0)
                    {
                        RenderExtraItems = 0;
                    }
                    RenderItem(GetItemForSlot(QuickBarPos - 5), new Location(center - (itemScale + itemScale + itemScale + itemScale + itemScale + itemScale + 3), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                    RenderItem(GetItemForSlot(QuickBarPos - 4), new Location(center - (itemScale + itemScale + itemScale + itemScale + itemScale + 3), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                    RenderItem(GetItemForSlot(QuickBarPos - 3), new Location(center - (itemScale + itemScale + itemScale + itemScale + 3), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                    RenderItem(GetItemForSlot(QuickBarPos + 3), new Location(center + (itemScale + itemScale + itemScale + 2), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                    RenderItem(GetItemForSlot(QuickBarPos + 4), new Location(center + (itemScale + itemScale + itemScale + itemScale + 2), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                    RenderItem(GetItemForSlot(QuickBarPos + 5), new Location(center + (itemScale + itemScale + itemScale + itemScale + itemScale + 2), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                }
                RenderItem(GetItemForSlot(QuickBarPos - 2), new Location(center - (itemScale + itemScale + itemScale + 3), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                RenderItem(GetItemForSlot(QuickBarPos - 1), new Location(center - (itemScale + itemScale + 2), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                RenderItem(GetItemForSlot(QuickBarPos + 1), new Location(center + (itemScale + 1), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                RenderItem(GetItemForSlot(QuickBarPos + 2), new Location(center + (itemScale + itemScale + 2), Window.Height - (itemScale + 16 + bottomup), 0), itemScale, sub3d);
                RenderItem(GetItemForSlot(QuickBarPos), new Location(center - (itemScale + 1), Window.Height - (itemScale * 2 + bottomup), 0), itemScale * 2, sub3d);
                if (!sub3d)
                {
                    string it = "^%^e^7" + GetItemForSlot(QuickBarPos).DisplayName;
                    float size = FontSets.Standard.MeasureFancyText(it);
                    FontSets.Standard.DrawColoredText(it, new Location(center - size / 2f, Window.Height - (itemScale * 2 + bottomup) - FontSets.Standard.font_default.Height - 5, 0));
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
                        FontSets.Standard.DrawColoredText(CameraDistance.ToString("0.0"), new Location(cX + move + CVars.u_reticlescale.ValueI, cY + move + CVars.u_reticlescale.ValueI, 0));
                    }
                    if (CVars.u_showcompass.ValueB)
                    {
                        Textures.White.Bind();
                        Rendering.SetColor(Color4.Black);
                        Rendering.RenderRectangle(64, Window.Height - (32 + 32), Window.Width - 64, Window.Height - 32);
                        Rendering.SetColor(Color4.Gray);
                        Rendering.RenderRectangle(66, Window.Height - (32 + 30), Window.Width - 66, Window.Height - 34);
                        Rendering.SetColor(Color4.White);
                        RenderCompassCoord(Vector4d.UnitY, "N");
                        RenderCompassCoord(-Vector4d.UnitY, "S");
                        RenderCompassCoord(Vector4d.UnitX, "E");
                        RenderCompassCoord(-Vector4d.UnitX, "W");
                        RenderCompassCoord(new Vector4d(1, 1, 0, 0), "NE");
                        RenderCompassCoord(new Vector4d(1, -1, 0, 0), "SE");
                        RenderCompassCoord(new Vector4d(-1, 1, 0, 0), "NW");
                        RenderCompassCoord(new Vector4d(-1, -1, 0, 0), "SW");
                    }
                }
            }
            if (!sub3d)
            {
                CScreen.FullRender(gDelta, 0, 0);
                //RenderInvMenu();
                //RenderChatSystem();
            }
            if (sub3d)
            {
                FixPersp = Matrix4.Identity;
            }
        }

        public void RenderCompassCoord(Vector4d rel, string dir)
        {
            Vector4d camp = new Vector4d(ClientUtilities.ConvertD(PlayerEyePosition), 1.0);
            Vector4d north = Vector4d.Transform(camp + rel * 10, MainWorldView.PrimaryMatrixd);
            double northOnScreen = north.X / north.W;
            if (north.Z <= 0 && northOnScreen < 0)
            {
                northOnScreen = -1f;
            }
            else if (north.Z <= 0 && northOnScreen > 0)
            {
                northOnScreen = 1f;
            }
            northOnScreen = Math.Max(100, Math.Min(Window.Width - 100, (0.5f + northOnScreen * 0.5f) * Window.Width));
            FontSets.Standard.DrawColoredText(dir, new Location(northOnScreen, Window.Height - (32 + 28), 0));
        }

        public bool IsOrtho = false;

        /// <summary>
        /// Renders an item on the 2D screen.
        /// </summary>
        /// <param name="item">The item to render.</param>
        /// <param name="pos">Where to render it.</param>
        /// <param name="size">How big to render it, in pixels.</param>
        public void RenderItem(ItemStack item, Location pos, int size, bool sub3d)
        {
            if (sub3d)
            {
                IsOrtho = true;
                item.Render3D(pos + new Location(size * 0.5f), (float)GlobalTickTimeLocal * 0.5f, new Location(size * 0.75));
                IsOrtho = false;
                return;
            }
            ItemFrame.Bind();
            Rendering.SetColor(Color4.White);
            Rendering.RenderRectangle((int)pos.X - 1, (int)pos.Y - 1, (int)(pos.X + size) + 1, (int)(pos.Y + size) + 1);
            item.Render(pos, new Location(size, size, 0));
            if (item.Count > 0)
            {
                FontSets.SlightlyBigger.DrawColoredText("^!^e^7^S" + item.Count, new Location(pos.X + 5, pos.Y + size - FontSets.SlightlyBigger.font_default.Height / 2f - 5, 0));
            }
        }
        
        public Matrix4 Ortho;
    }

}
