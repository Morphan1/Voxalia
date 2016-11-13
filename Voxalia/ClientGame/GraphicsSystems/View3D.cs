//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.Shared;
using System.Diagnostics;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems
{
    public class View3D
    {
        /// <summary>
        /// Set this to whatever method call renders all 3D objects in this view.
        /// </summary>
        public Action<View3D> Render3D = null;

        public Action PostFirstRender = null;

        public bool ShadowsOnly = false;

        public bool ShadowingAllowed = false;

        public bool TranspShadows = false;

        public FBOID FBOid = FBOID.NONE;

        public int CurrentFBO = 0;

        public int CurrentFBOTexture = 0;

        public int CurrentFBODepth = 0;

        public bool RenderSpecular = false;

        public int Width;

        public int Height;

        public Material Headmat = Material.AIR;

        public Location SunLoc = Location.NaN;

        public Client TheClient;

        int fbo_texture;
        int fbo_main;

        int fbo_godray_main;
        int fbo_godray_texture;
        int fbo_godray_texture2;

        RenderSurface4Part RS4P;

        public double ShadowTime;
        public double FBOTime;
        public double LightsTime;
        public double TotalTime;

        public double ShadowSpikeTime;
        public double FBOSpikeTime;
        public double LightsSpikeTime;
        public double TotalSpikeTime;

        public Func<int> ShadowTexSize = () => 64;

        public Matrix4d OffsetWorld = Matrix4d.Identity;

        public const int MAT_LOC_VIEW = 1;

        public const int MAT_LOC_OBJECT = 2;

        public void SetMatrix(int mat_loc, Matrix4d mat)
        {
            Matrix4d temp = mat;
            if (!TheClient.IsOrtho)
            {
                temp = temp * OffsetWorld;
            }
            Matrix4 mat4f = new Matrix4((float)temp.M11, (float)temp.M12, (float)temp.M13, (float)temp.M14, (float)temp.M21, (float)temp.M22, (float)temp.M23, (float)temp.M24,
                (float)temp.M31, (float)temp.M32, (float)temp.M33, (float)temp.M34, (float)temp.M41, (float)temp.M42, (float)temp.M43, (float)temp.M44);
            GL.UniformMatrix4(mat_loc, false, ref mat4f);
        }

        public void FinalHDRGrab()
        {
            if (TheClient.CVars.r_hdr.ValueB)
            {
                float[] rd = new float[HDR_SPREAD * HDR_SPREAD];
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, hdrfbo);
                GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                GL.ReadPixels(0, 0, HDR_SPREAD, HDR_SPREAD, PixelFormat.Red, PixelType.Float, rd);
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                GL.ReadBuffer(ReadBufferMode.None);
                float exp = FindExp(rd);
                exp = Math.Max(Math.Min(exp, 3.0f), 0.33f);
                exp = 1.0f / exp;
                float stepUp = (float)TheClient.gDelta * 0.05f;
                float stepDown = stepUp * 5.0f;
                if (exp > MainEXP + stepUp)
                {
                    MainEXP += stepUp;
                }
                else if (exp < MainEXP - stepDown)
                {
                    MainEXP -= stepDown;
                }
                else
                {
                    MainEXP = exp;
                }
            }
            else
            {
                MainEXP = 0.75f;
            }
        }

        const int HDR_SPREAD = 4;

        public void GenerateLightHelpers()
        {
            if (RS4P != null)
            {
                RS4P.Destroy();
                GL.DeleteFramebuffer(fbo_main);
                GL.DeleteTexture(fbo_texture);
                RS4P = null;
                fbo_main = 0;
                fbo_texture = 0;
                GL.DeleteFramebuffer(fbo_godray_main);
                GL.DeleteTexture(fbo_godray_texture);
                GL.DeleteTexture(fbo_godray_texture2);
                GL.DeleteFramebuffer(hdrfbo);
                GL.DeleteTexture(hdrtex);
                GL.DeleteFramebuffers(LIGHTS_MAX, fbo_shadow);
                GL.DeleteTextures(LIGHTS_MAX, fbo_shadow_depth);
                GL.DeleteTexture(fbo_shadow_tex);
            }
            CheckError("Load - View3D - Light - Deletes");
            RS4P = new RenderSurface4Part(Width, Height, TheClient.Rendering);
            // FBO
            fbo_texture = GL.GenTexture();
            fbo_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            CheckError("Load - View3D - Light - FBO");
            // Godray FBO
            fbo_godray_texture = GL.GenTexture();
            fbo_godray_texture2 = GL.GenTexture();
            fbo_godray_main = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Bgra, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_godray_texture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, TextureTarget.Texture2D, fbo_godray_texture2, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            CheckError("Load - View3D - Light - Godray");
            // HDR FBO
            hdrtex = GL.GenTexture();
            hdrfbo = GL.GenFramebuffer();
            GL.BindTexture(TextureTarget.Texture2D, hdrtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrfbo);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, hdrtex, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            CheckError("Load - View3D - Light - HDR");
            // Shadow FBO
            int sq = ShadowTexSize();
            fbo_shadow_tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, sq, sq, LIGHTS_MAX, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CheckError("Load - View3D - Light - Shadows");
            for (int i = 0; i < LIGHTS_MAX; i++)
            {
                fbo_shadow[i] = GL.GenFramebuffer();
                fbo_shadow_depth[i] = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, fbo_shadow_depth[i]);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, sq, sq, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[i]);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, fbo_shadow_depth[i], 0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[i]);
                GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, fbo_shadow_tex, 0, i);
                CheckError("Load - View3D - Light - LMAP:" + i);
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            CheckError("Load - View3D - Light - Final");
        }

        int[] fbo_shadow = new int[LIGHTS_MAX];
        int fbo_shadow_tex = -1;
        int[] fbo_shadow_depth = new int[LIGHTS_MAX];

        int hdrfbo;
        int hdrtex;

        public void GenerateFBO()
        {
            if (CurrentFBO != 0)
            {
                GL.DeleteFramebuffer(CurrentFBO);
                GL.DeleteTexture(CurrentFBOTexture);
                GL.DeleteTexture(CurrentFBODepth);
            }
            GL.ActiveTexture(TextureUnit.Texture0);
            CurrentFBOTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, CurrentFBOTexture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CurrentFBODepth = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, CurrentFBODepth);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            CurrentFBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, CurrentFBOTexture, 0);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, CurrentFBODepth, 0);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        
        int transp_fbo_main = 0;
        int transp_fbo_texture = 0;
        int transp_fbo_depthtex = 0;

        public void Generate(Client tclient, int w, int h)
        {
            TheClient = tclient;
            Width = w;
            Height = h;
            GenerateLightHelpers();
            CheckError("Load - View3D - Light");
            GenerateTranspHelpers();
            CheckError("Load - View3D - Transp");
        }

        public void GenerateTranspHelpers()
        {
            if (transp_fbo_main != 0)
            {
                GL.DeleteFramebuffer(transp_fbo_main);
                GL.DeleteTexture(transp_fbo_texture);
                GL.DeleteTexture(transp_fbo_depthtex);
            }
            // TODO: Helper class!
            transp_fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba32f, Width, Height, 0, PixelFormat.Rgba, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            transp_fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, transp_fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, Width, Height, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
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
            // Linked list stuff
            // TODO: Regeneratable, on window resize in particular.
            if (LLActive)
            {
                // TODO: If was active, delete old data
                GenTexture();
                GenBuffer(1, false);
                GenBuffer(2, true);
                GL.ActiveTexture(TextureUnit.Texture7);
                int cspb = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, cspb);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)sizeof(uint), IntPtr.Zero, BufferUsageHint.StaticDraw);
                int csp = GL.GenTexture();
                GL.BindTexture(TextureTarget.TextureBuffer, csp);
                GL.TexBuffer(TextureBufferTarget.TextureBuffer, SizedInternalFormat.R32f, cspb);
                GL.BindImageTexture(5, csp, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                TransTexs[3] = csp;
                GL.BindTexture(TextureTarget.TextureBuffer, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
            }
        }

        public bool LLActive = false;

        int[] TransTexs = new int[4];

        public int GenTexture()
        {
            GL.ActiveTexture(TextureUnit.Texture4);
            int temp = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, temp);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, Width, Height, 3, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.BindImageTexture(4, temp, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
            TransTexs[0] = temp;
            //GL.BindTexture(TextureTarget.Texture2DArray, 0);
            return temp;
        }

        public int AB_SIZE = 8 * 1024 * 1024; // TODO: Tweak me!
        public const int P_SIZE = 4;

        public int GenBuffer(int c, bool flip)
        {
            GL.ActiveTexture(TextureUnit.Texture4 + c);
            int temp = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.TextureBuffer, temp);
            GL.BufferData(BufferTarget.TextureBuffer, (IntPtr)(flip ? AB_SIZE / P_SIZE * sizeof(uint) : AB_SIZE * sizeof(float) * 4), IntPtr.Zero, BufferUsageHint.StaticDraw);
            int ttex = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureBuffer, ttex);
            GL.TexBuffer(TextureBufferTarget.TextureBuffer, flip ? SizedInternalFormat.R32f : SizedInternalFormat.Rgba32f, temp);
            GL.BindImageTexture(4 + c, ttex, 0, false, 0, TextureAccess.ReadWrite, flip ? SizedInternalFormat.R32ui : SizedInternalFormat.Rgba32f);
            TransTexs[c] = ttex;
            //GL.BindTexture(TextureTarget.TextureBuffer, 0);
            return temp;
        }

        public Func<Location> CameraUp = () => Location.UnitZ;

        public Location ambient;

        public float DesaturationAmount = 0f;

        public Vector3 DesaturationColor = new Vector3(0.95f, 0.77f, 0.55f);

        void SetViewport()
        {
            GL.Viewport(0, 0, Width, Height);
        }

        public Location CameraPos;

        public Location CameraTarget;
        public const float LightMaximum = 1E10f;

        public void StandardBlend()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void TranspBlend()
        {
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
        }

        public List<LightObject> Lights = new List<LightObject>();

        public bool RenderTextures = false;

        public bool RenderingShadows;

        public Location ForwardVec = Location.Zero;
        
        public Matrix4 PrimaryMatrix;

        public Frustum cf2;

        public Frustum CFrust;

        public int LightsC = 0;

        public float[] ClearColor = new float[] { 0f, 1f, 1f, 0f };

        public static void CheckError(string loc)
        {
#if DEBUG
            ErrorCode ec = GL.GetError();
            while (ec != ErrorCode.NoError)
            {
                SysConsole.Output(OutputType.ERROR, "OpenGL error [" + loc + "]: " + ec);
                ec = GL.GetError();
            }
#endif
        }

        public float RenderClearAlpha = 1f;

        public float MainEXP = 1.0f;
        
        public float FindExp(float[] inp)
        {
            float total = 0f;
            for (int i = 0; i < inp.Length; i++)
            {
                total += inp[i];
            }
            return total / (float)inp.Length;
        }

        public bool FastOnly = false;

        public void Render()
        {
            try
            {
                RenderPass_Setup();
                if (FastOnly || TheClient.CVars.r_fast.ValueB)
                {
                    RenderPass_FAST();
                    return;
                }
                Stopwatch timer = new Stopwatch();
                timer.Start();
                if (TheClient.CVars.r_shadows.ValueB)
                {
                    RenderPass_Shadows();
                }
                RenderPass_GBuffer();
                RenderPass_Lights();
                FinalHDRGrab();
                PForward = CameraForward + CameraPos;
                timer.Stop();
                TotalTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (TotalTime > TotalSpikeTime)
                {
                    TotalSpikeTime = TotalTime;
                }
            }
            catch (Exception ex)
            {
                Utilities.CheckException(ex);
                SysConsole.Output("Rendering (3D)", ex);
            }
        }

        public Location CameraForward = Location.UnitX;

        public static Matrix4 IdentityMatrix = Matrix4.Identity;

        Location cameraBasePos;

        Location cameraAdjust;

        Matrix4 PrimaryMatrix_OffsetFor3D;

        Location PForward = Location.Zero;

        public Func<BEPUutilities.Quaternion> CameraModifier = () => BEPUutilities.Quaternion.Identity;

        public Location CalcForward()
        {
            BEPUutilities.Quaternion cammod = CameraModifier();
            Location camforward = ForwardVec;
            camforward = new Location(BEPUutilities.Quaternion.Transform(camforward.ToBVector(), cammod));
            return camforward;
        }

        /// <summary>
        /// Set up the rendering engine.
        /// </summary>
        public void RenderPass_Setup()
        {
            Location camup = CameraUp();
            BEPUutilities.Quaternion cammod = CameraModifier();
            Location camforward = ForwardVec;
            camup = new Location(BEPUutilities.Quaternion.Transform(camup.ToBVector(), cammod));
            camforward = new Location(BEPUutilities.Quaternion.Transform(camforward.ToBVector(), cammod));
            CameraForward = camforward;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            GL.DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            StandardBlend();
            GL.Enable(EnableCap.DepthTest);
            RenderTextures = true;
            GL.ClearBuffer(ClearBuffer.Color, 0, ClearColor);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
            cameraBasePos = CameraPos;
            cameraAdjust = -camforward.CrossProduct(camup) * 0.25;
            if (TheClient.VR != null)
            {
                cameraAdjust = -cameraAdjust;
            }
            RenderRelative = CameraPos;
            SetViewport();
            CameraTarget = CameraPos + camforward;
            OffsetWorld = Matrix4d.CreateTranslation(ClientUtilities.ConvertD(-CameraPos));
            if (TheClient.VR != null)
            {
                /*
                Matrix4 proj = TheClient.VR.GetProjection(true, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF);
                Matrix4 view = TheClient.VR.Eye(true);
                PrimaryMatrix = view * proj;
                Matrix4 proj2 = TheClient.VR.GetProjection(false, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF);
                Matrix4 view2 = TheClient.VR.Eye(false);
                PrimaryMatrix_OffsetFor3D = view2 * proj2;
                PrimaryMatrixd = new Matrix4d(PrimaryMatrix.M11, PrimaryMatrix.M12, PrimaryMatrix.M13, PrimaryMatrix.M14, PrimaryMatrix.M21, PrimaryMatrix.M22, PrimaryMatrix.M23, PrimaryMatrix.M24,
                    PrimaryMatrix.M31, PrimaryMatrix.M32, PrimaryMatrix.M33, PrimaryMatrix.M34, PrimaryMatrix.M41, PrimaryMatrix.M42, PrimaryMatrix.M43, PrimaryMatrix.M44)
                    * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(CameraPos));
                PrimaryMatrix_OffsetFor3Dd = new Matrix4d(PrimaryMatrix_OffsetFor3D.M11, PrimaryMatrix_OffsetFor3D.M12, PrimaryMatrix_OffsetFor3D.M13, PrimaryMatrix_OffsetFor3D.M14, PrimaryMatrix_OffsetFor3D.M21, PrimaryMatrix_OffsetFor3D.M22, PrimaryMatrix_OffsetFor3D.M23, PrimaryMatrix_OffsetFor3D.M24,
                    PrimaryMatrix_OffsetFor3D.M31, PrimaryMatrix.M32, PrimaryMatrix_OffsetFor3D.M33, PrimaryMatrix_OffsetFor3D.M34, PrimaryMatrix_OffsetFor3D.M41, PrimaryMatrix_OffsetFor3D.M42, PrimaryMatrix_OffsetFor3D.M43, PrimaryMatrix_OffsetFor3D.M44)
                    * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(CameraPos));
                */
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(TheClient.CVars.r_fov.ValueF),
                    (float)Width / (float)Height, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF); // TODO: View3D-level vars?
                Location bx = cameraAdjust;
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(bx), ClientUtilities.Convert(bx + camforward), ClientUtilities.Convert(camup));
                PrimaryMatrix = view * proj;
                Matrix4 view2 = Matrix4.LookAt(ClientUtilities.Convert(-cameraAdjust), ClientUtilities.Convert(-cameraAdjust + camforward), ClientUtilities.Convert(camup));
                PrimaryMatrix_OffsetFor3D = view2 * proj;
                Matrix4d projd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(TheClient.CVars.r_fov.ValueF),
                    (float)Width / (float)Height, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF); // TODO: View3D-level vars?
                Location bxd = CameraPos + cameraAdjust;
                Matrix4d viewd = Matrix4d.LookAt(ClientUtilities.ConvertD(bxd), ClientUtilities.ConvertD(bxd + camforward), ClientUtilities.ConvertD(camup));
                PrimaryMatrixd = viewd * projd;
                PrimaryMatrix_OffsetFor3Dd = Matrix4d.Identity;
                Matrix4d view2d = Matrix4d.LookAt(ClientUtilities.ConvertD(CameraPos - cameraAdjust), ClientUtilities.ConvertD(CameraPos - cameraAdjust + camforward), ClientUtilities.ConvertD(camup));
                PrimaryMatrix_OffsetFor3Dd = view2d * projd;
            }
            else
            {
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(TheClient.CVars.r_fov.ValueF),
                    (float)Width / (float)Height, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF); // TODO: View3D-level vars?
                Location bx = TheClient.CVars.r_3d_enable.ValueB ? (cameraAdjust) : Location.Zero;
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(bx), ClientUtilities.Convert(bx + camforward), ClientUtilities.Convert(camup));
                PrimaryMatrix = view * proj;
                if (TheClient.CVars.r_3d_enable.ValueB)
                {
                    Matrix4 view2 = Matrix4.LookAt(ClientUtilities.Convert(-cameraAdjust), ClientUtilities.Convert(-cameraAdjust + camforward), ClientUtilities.Convert(camup));
                    PrimaryMatrix_OffsetFor3D = view2 * proj;
                }
                Matrix4d projd = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(TheClient.CVars.r_fov.ValueF),
                    (float)Width / (float)Height, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF); // TODO: View3D-level vars?
                Location bxd = TheClient.CVars.r_3d_enable.ValueB ? (CameraPos + cameraAdjust) : CameraPos;
                Matrix4d viewd = Matrix4d.LookAt(ClientUtilities.ConvertD(bxd), ClientUtilities.ConvertD(bxd + camforward), ClientUtilities.ConvertD(camup));
                PrimaryMatrixd = viewd * projd;
                PrimaryMatrix_OffsetFor3Dd = Matrix4d.Identity;
                if (TheClient.CVars.r_3d_enable.ValueB)
                {
                    Matrix4d view2d = Matrix4d.LookAt(ClientUtilities.ConvertD(CameraPos - cameraAdjust), ClientUtilities.ConvertD(CameraPos - cameraAdjust + camforward), ClientUtilities.ConvertD(camup));
                    PrimaryMatrix_OffsetFor3Dd = view2d * projd;
                }
            }
            camFrust = new Frustum(PrimaryMatrixd);
            cf2 = new Frustum(PrimaryMatrix_OffsetFor3Dd);
            CFrust = camFrust;
            CheckError("AfterSetup");
        }

        public Matrix4d PrimaryMatrixd;

        public Matrix4d PrimaryMatrix_OffsetFor3Dd;

        /// <summary>
        /// Render everything as quickly as possible: a simple forward renderer.
        /// </summary>
        public void RenderPass_FAST()
        {
            RenderingShadows = false;
            GL.ActiveTexture(TextureUnit.Texture0);
            FBOid = FBOID.FORWARD_SOLID;
            TheClient.s_forw_vox.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.s_forw.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            TheClient.Rendering.SetColor(Color4.White);
            if (TheClient.CVars.r_3d_enable.ValueB || TheClient.VR != null)
            {
                GL.Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                GL.Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                TheClient.s_forw_vox = TheClient.s_forw_vox.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                TheClient.s_forw = TheClient.s_forw.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Render3D(this);
                GL.Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            FBOid = FBOID.FORWARD_TRANSP;
            TheClient.s_forw_vox_trans.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            TheClient.Rendering.SetColor(Color4.White);
            TheClient.s_forw_trans.Bind();
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            TheClient.Rendering.SetColor(Color4.White);
            if (PostFirstRender != null)
            {
                PostFirstRender();
            }
            GL.DepthMask(false);
            if (TheClient.CVars.r_3d_enable.ValueB || TheClient.VR != null)
            {
                GL.Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                GL.Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                TheClient.s_forw_vox_trans = TheClient.s_forw_vox_trans.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                TheClient.s_forw_trans = TheClient.s_forw_trans.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Render3D(this);
                GL.Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            GL.DepthMask(true);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
            CheckError("AfterFast");
        }

        public Location RenderRelative;

        /// <summary>
        /// Calculate shadow maps for the later (lighting) render passes.
        /// </summary>
        public void RenderPass_Shadows()
        {
            if (TheClient.shouldRedrawShadows && ShadowingAllowed)
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();
                TheClient.s_shadow = TheClient.s_shadow.Bind();
                TheClient.FixPersp = Matrix4.Identity;
                RenderingShadows = true;
                ShadowsOnly = true;
                LightsC = 0;
                Location campos = CameraPos;
                int n = 0;
                Frustum tcf = CFrust;
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        if (Lights[i] is SkyLight || Lights[i].EyePos.DistanceSquared(campos) <
                            TheClient.CVars.r_lightmaxdistance.ValueD * TheClient.CVars.r_lightmaxdistance.ValueD + Lights[i].MaxDistance * Lights[i].MaxDistance * 6)
                        {
                            LightsC++;
                            for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                            {
                                if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                {
                                    continue;
                                }
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[n]);
                                if (Lights[i].InternalLights[x] is LightOrtho)
                                {
                                    CFrust = null;
                                }
                                else
                                {
                                    CFrust = new Frustum(ClientUtilities.ConvertToD(Lights[i].InternalLights[x].GetMatrix()));
                                }
                                CameraPos = ClientUtilities.ConvertD(Lights[i].InternalLights[x].eye);
                                TheClient.s_shadowvox = TheClient.s_shadowvox.Bind();
                                SetMatrix(2, Matrix4d.Identity);
                                Lights[i].InternalLights[x].SetProj();
                                if (Lights[i].InternalLights[x] is LightOrtho)
                                {
                                    GL.Uniform1(3, 1.0f);
                                }
                                else
                                {
                                    GL.Uniform1(3, 0.0f);
                                }
                                GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                FBOid = FBOID.SHADOWS;
                                TheClient.s_shadow = TheClient.s_shadow.Bind();
                                SetMatrix(2, Matrix4d.Identity);
                                if (Lights[i].InternalLights[x] is LightOrtho)
                                {
                                    GL.Uniform1(3, 1.0f);
                                }
                                else
                                {
                                    GL.Uniform1(3, 0.0f);
                                }
                                GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                TranspShadows = Lights[i].InternalLights[x].transp;
                                Lights[i].InternalLights[x].SetProj();
                                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_shadow[n]);
                                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1f });
                                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                                Render3D(this);
                                FBOid = FBOID.NONE;
                                n++;
                                if (n >= LIGHTS_MAX)
                                {
                                    goto complete;
                                }
                            }
                        }
                    }
                }
                complete:
                CFrust = tcf;
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
                GL.DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
                CameraPos = campos;
                RenderingShadows = false;
                ShadowsOnly = false;
                timer.Stop();
                ShadowTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (ShadowTime > ShadowSpikeTime)
                {
                    ShadowSpikeTime = ShadowTime;
                }
                StandardBlend();
                CheckError("AfterShadows");
            }
        }

        /// <summary>
        /// Generate the G-Buffer ("FBO") for lighting and final passes.
        /// </summary>
        public void RenderPass_GBuffer()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            SetViewport();
            TheClient.s_fbov = TheClient.s_fbov.Bind();
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform2(8, new Vector2(TheClient.sl_min, TheClient.sl_max));
            TheClient.s_fbot = TheClient.s_fbot.Bind();
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            TheClient.s_fbo = TheClient.s_fbo.Bind();
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            FBOid = FBOID.MAIN;
            RenderingShadows = false;
            CFrust = camFrust;
            GL.ActiveTexture(TextureUnit.Texture0);
            RS4P.Bind();
            RenderLights = true;
            RenderSpecular = true;
            TheClient.Rendering.SetColor(Color4.White);
            StandardBlend();
            if (TheClient.CVars.r_3d_enable.ValueB || TheClient.VR != null)
            {
                GL.Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                GL.Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                TheClient.s_fbov = TheClient.s_fbov.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix);
                TheClient.s_fbot = TheClient.s_fbot.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix);
                TheClient.s_fbo = TheClient.s_fbo.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix);
                Render3D(this);
                GL.Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            CheckError("AfterFBO");
            RenderPass_RefractionBuffer(timer);
        }

        /// <summary>
        /// Adds refraction data to the G-Buffer ("FBO").
        /// </summary>
        public void RenderPass_RefractionBuffer(Stopwatch timer)
        {
            FBOid = FBOID.REFRACT;
            TheClient.s_fbov_refract = TheClient.s_fbov_refract.Bind();
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform2(8, new Vector2(TheClient.sl_min, TheClient.sl_max));
            TheClient.s_fbo_refract = TheClient.s_fbo_refract.Bind();
            GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.DepthMask(false);
            if (TheClient.CVars.r_3d_enable.ValueB || TheClient.VR != null)
            {
                GL.Viewport(Width / 2, 0, Width / 2, Height);
                Render3D(this);
                CFrust = cf2;
                GL.Viewport(0, 0, Width / 2, Height);
                CameraPos = cameraBasePos - cameraAdjust;
                TheClient.s_fbov_refract = TheClient.s_fbov_refract.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                TheClient.s_fbo_refract = TheClient.s_fbo_refract.Bind();
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                Render3D(this);
                GL.Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                Render3D(this);
            }
            CheckError("AfterRefract");
            GL.DepthMask(true);
            RenderLights = false;
            RenderSpecular = false;
            RS4P.Unbind();
            FBOid = FBOID.NONE;
            timer.Stop();
            FBOTime = (double)timer.ElapsedMilliseconds / 1000f;
            if (FBOTime > FBOSpikeTime)
            {
                FBOSpikeTime = FBOTime;
            }
        }

        public Matrix4 SimpleOrthoMatrix = Matrix4.CreateOrthographicOffCenter(-1, 1, -1, 1, -1, 1);

        public Frustum camFrust;

        const int LIGHTS_MAX = 10;

        public void RenderPass_Lights()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, RenderClearAlpha });
            if (TheClient.CVars.r_shadows.ValueB)
            {
                TheClient.s_shadowadder = TheClient.s_shadowadder.Bind();
                GL.Uniform1(3, TheClient.CVars.r_shadowblur.ValueF);
            }
            else
            {
                TheClient.s_lightadder = TheClient.s_lightadder.Bind();
            }
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.PositionTexture);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.NormalsTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.RenderhintTexture);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
            GL.Uniform3(4, ClientUtilities.Convert(ambient));
            GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest);
            TranspBlend();
            if (TheClient.CVars.r_lighting.ValueB)
            {
                float[] light_dat = new float[LIGHTS_MAX * 16];
                float[] shadowmat_dat = new float[LIGHTS_MAX * 16];
                int c = 0;
                // TODO: An ambient light source?
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        double d1 = (Lights[i].EyePos - CameraPos).LengthSquared();
                        double d2 = TheClient.CVars.r_lightmaxdistance.ValueD * TheClient.CVars.r_lightmaxdistance.ValueD + Lights[i].MaxDistance * Lights[i].MaxDistance;
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
                            for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                            {
                                if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                {
                                    continue;
                                }
                                Matrix4 smat = Lights[i].InternalLights[x].GetMatrix();
                                Vector3d eyep = Lights[i].InternalLights[x].eye - ClientUtilities.ConvertD(CameraPos);
                                Vector3 col = Lights[i].InternalLights[x].color * (float)maxrangemult;
                                Matrix4 light_data = new Matrix4(
                                    (float)eyep.X, (float)eyep.Y, (float)eyep.Z, // light_pos
                                    0.7f, // diffuse_albedo
                                    0.7f, // specular_albedo
                                    Lights[i].InternalLights[x] is LightOrtho ? 1.0f : 0.0f, // should_sqrt
                                    col.X, col.Y, col.Z, // light_color
                                    Lights[i].InternalLights[x] is LightOrtho ? LightMaximum : (Lights[i].InternalLights[0].maxrange <= 0 ? LightMaximum : Lights[i].InternalLights[0].maxrange), // light_radius
                                    0f, 0f, 0f, // eye_pos
                                    Lights[i] is SpotLight ? 1.0f : 0.0f, // light_type
                                    1f / ShadowTexSize(), // tex_size
                                    0.0f // Unused.
                                    );
                                for (int mx = 0; mx < 4; mx++)
                                {
                                    for (int my = 0; my < 4; my++)
                                    {
                                        shadowmat_dat[c * 16 + mx * 4 + my] = smat[mx, my];
                                        light_dat[c * 16 + mx * 4 + my] = light_data[mx, my];
                                    }
                                }
                                c++;
                                if (c >= LIGHTS_MAX)
                                {
                                    goto lights_apply;
                                }
                            }
                        }
                    }
                }
                lights_apply:
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.Uniform1(9, (float)c);
                GL.UniformMatrix4(10, LIGHTS_MAX, false, shadowmat_dat);
                GL.UniformMatrix4(10 + LIGHTS_MAX, LIGHTS_MAX, false, light_dat);
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                StandardBlend();
                CheckError("AfterLighting");
                RenderPass_HDR();
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, hdrtex);
                float[] data = new float[HDR_SPREAD * HDR_SPREAD];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 1f;
                }
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, data);
            }
            CheckError("AfterAllLightCode");
            RenderPass_LightsToBase();
            int lightc = RenderPass_Transparents();
            RenderPass_Bloom(lightc);
            timer.Stop();
            LightsTime = (double)timer.ElapsedMilliseconds / 1000f;
            if (LightsTime > LightsSpikeTime)
            {
                LightsSpikeTime = LightsTime;
            }
            timer.Reset();
            CheckError("AtEnd");
        }
        
        /// <summary>
        /// Calculates the brightness value for High Dynamic Range rendering.
        /// </summary>
        public void RenderPass_HDR()
        {
            if (TheClient.CVars.r_lighting.ValueB && TheClient.CVars.r_hdr.ValueB)
            {
                TheClient.s_hdrpass.Bind();
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, hdrfbo);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                StandardBlend();
                CheckError("AfterHDRRead");
            }
            else
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, hdrtex);
                float[] data = new float[HDR_SPREAD * HDR_SPREAD];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = 1f;
                }
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, HDR_SPREAD, HDR_SPREAD, 0, PixelFormat.Red, PixelType.Float, data);
            }
        }

        public void RenderPass_LightsToBase()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
            if (TheClient.CVars.r_lighting.ValueB)
            {
                if (TheClient.CVars.r_toonify.ValueB)
                {
                    TheClient.s_finalgodray_lights_toonify = TheClient.s_finalgodray_lights_toonify.Bind();
                }
                else
                {
                    if (TheClient.CVars.r_motionblur.ValueB)
                    {
                        TheClient.s_finalgodray_lights_motblur = TheClient.s_finalgodray_lights_motblur.Bind();
                    }
                    else
                    {
                        TheClient.s_finalgodray_lights = TheClient.s_finalgodray_lights.Bind();
                    }
                }
            }
            else
            {
                if (TheClient.CVars.r_toonify.ValueB)
                {
                    TheClient.s_finalgodray_toonify = TheClient.s_finalgodray_toonify.Bind();
                }
                else
                {
                    TheClient.s_finalgodray = TheClient.s_finalgodray.Bind();
                }
            }
            GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 0f, 0f, 0f, 0f });
            GL.BlendFuncSeparate(1, BlendingFactorSrc.SrcColor, BlendingFactorDest.Zero, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
            GL.Uniform3(8, ClientUtilities.Convert(TheClient.CameraFinalTarget));
            GL.Uniform1(9, TheClient.CVars.r_dof_strength.ValueF);
            GL.Uniform1(10, MainEXP);
            GL.Uniform1(16, TheClient.CVars.r_znear.ValueF);
            GL.Uniform1(17, TheClient.CVars.r_zfar.ValueF);
            GL.Uniform4(18, new Vector4(ClientUtilities.Convert(Headmat.GetFogColor()), (float)Headmat.GetFogAlpha()));
            GL.Uniform1(19, DesaturationAmount);
            GL.Uniform3(20, new Vector3(0, 0, 0));
            GL.Uniform3(21, DesaturationColor);
            GL.UniformMatrix4(22, false, ref PrimaryMatrix);
            GL.Uniform1(24, (float)Width);
            GL.Uniform1(25, (float)Height);
            GL.Uniform1(26, (float)TheClient.GlobalTickTimeLocal);
            Vector4 v = Vector4.Transform(new Vector4(ClientUtilities.Convert(PForward), 1f), PrimaryMatrix);
            Vector2 v2 = (v.Xy / v.W);
            Vector2 rel = (pfRes - v2) * 0.01f;
            if (float.IsNaN(rel.X) || float.IsInfinity(rel.X) || float.IsNaN(rel.Y) || float.IsInfinity(rel.Y))
            {
                rel = new Vector2(0f, 0f);
            }
            GL.Uniform2(27, ref rel);
            pfRes = v2;
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.ActiveTexture(TextureUnit.Texture7);
            GL.BindTexture(TextureTarget.Texture2D, hdrtex);
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.Rh2Texture);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
            GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            CheckError("FirstRenderToBasePassPre");
            TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
            CheckError("FirstRenderToBasePassComplete");
            GL.ActiveTexture(TextureUnit.Texture6);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture5);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            CheckError("AmidTextures");
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Enable(EnableCap.DepthTest);
            CheckError("PreBlendFunc");
            //GL.BlendFunc(1, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            CheckError("PreAFRFBO");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            GL.DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            CheckError("AFRFBO_1");
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo); // TODO: is this line and line below needed?
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            CheckError("AFRFBO_2");
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo_godray_main);
            CheckError("AFRFBO_3");
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
            CheckError("AFRFBO_4");
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.Enable(EnableCap.CullFace);
            CheckError("AfterFirstRender");
            if (PostFirstRender != null)
            {
                PostFirstRender();
            }
            CheckError("AfterPostFirstRender");
        }

        Vector2 pfRes = Vector2.Zero;

        public int RenderPass_Transparents()
        {
            if (TheClient.CVars.r_transplighting.ValueB)
            {
                if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlyvoxlitsh_ll = TheClient.s_transponlyvoxlitsh_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlyvoxlitsh = TheClient.s_transponlyvoxlitsh.Bind();
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlyvoxlit_ll = TheClient.s_transponlyvoxlit_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlyvoxlit = TheClient.s_transponlyvoxlit.Bind();
                    }
                }
            }
            else
            {
                if (TheClient.CVars.r_transpll.ValueB)
                {
                    TheClient.s_transponlyvox_ll = TheClient.s_transponlyvox_ll.Bind();
                }
                else
                {
                    TheClient.s_transponlyvox = TheClient.s_transponlyvox.Bind();
                }
            }
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(4, DesaturationAmount);
            if (TheClient.CVars.r_transplighting.ValueB)
            {
                if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlylitsh_ll = TheClient.s_transponlylitsh_ll.Bind();
                        FBOid = FBOID.TRANSP_SHADOWS_LL;
                    }
                    else
                    {
                        TheClient.s_transponlylitsh = TheClient.s_transponlylitsh.Bind();
                        FBOid = FBOID.TRANSP_SHADOWS;
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlylit_ll = TheClient.s_transponlylit_ll.Bind();
                        FBOid = FBOID.TRANSP_LIT_LL;
                    }
                    else
                    {
                        TheClient.s_transponlylit = TheClient.s_transponlylit.Bind();
                        FBOid = FBOID.TRANSP_LIT;
                    }
                }
            }
            else
            {
                if (TheClient.CVars.r_transpll.ValueB)
                {
                    TheClient.s_transponly_ll = TheClient.s_transponly_ll.Bind();
                    FBOid = FBOID.TRANSP_LL;
                }
                else
                {
                    TheClient.s_transponly = TheClient.s_transponly.Bind();
                    FBOid = FBOID.TRANSP_UNLIT;
                }
            }
            GL.UniformMatrix4(1, false, ref PrimaryMatrix);
            GL.UniformMatrix4(2, false, ref IdentityMatrix);
            GL.Uniform1(4, DesaturationAmount);
            GL.DepthMask(false);
            if (TheClient.CVars.r_transpll.ValueB)
            {
                StandardBlend();
            }
            else
            {
                TranspBlend();
            }
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, transp_fbo_main);
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, RS4P.fbo);
            GL.BlitFramebuffer(0, 0, Width, Height, 0, 0, Width, Height, ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
            int lightc = 0;
            CheckError("PreTransp");
            if (TheClient.CVars.r_3d_enable.ValueB || TheClient.VR != null)
            {
                GL.Viewport(Width / 2, 0, Width / 2, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                RenderTransp(ref lightc);
                CFrust = cf2;
                GL.Viewport(0, 0, Width / 2, Height);
                CFrust = cf2;
                if (TheClient.CVars.r_transplighting.ValueB)
                {
                    if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                    {
                        if (TheClient.CVars.r_transpll.ValueB)
                        {
                            TheClient.s_transponlyvoxlitsh_ll = TheClient.s_transponlyvoxlitsh_ll.Bind();
                        }
                        else
                        {
                            TheClient.s_transponlyvoxlitsh = TheClient.s_transponlyvoxlitsh.Bind();
                        }
                    }
                    else
                    {
                        if (TheClient.CVars.r_transpll.ValueB)
                        {
                            TheClient.s_transponlyvoxlit_ll = TheClient.s_transponlyvoxlit_ll.Bind();
                        }
                        else
                        {
                            TheClient.s_transponlyvoxlit = TheClient.s_transponlyvoxlit.Bind();
                        }
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlyvox_ll = TheClient.s_transponlyvox_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlyvox = TheClient.s_transponlyvox.Bind();
                    }
                }
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                if (TheClient.CVars.r_transplighting.ValueB)
                {
                    if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                    {
                        if (TheClient.CVars.r_transpll.ValueB)
                        {
                            TheClient.s_transponlylitsh_ll = TheClient.s_transponlylitsh_ll.Bind();
                            FBOid = FBOID.TRANSP_SHADOWS_LL;
                        }
                        else
                        {
                            TheClient.s_transponlylitsh = TheClient.s_transponlylitsh.Bind();
                            FBOid = FBOID.TRANSP_SHADOWS;
                        }
                    }
                    else
                    {
                        if (TheClient.CVars.r_transpll.ValueB)
                        {
                            TheClient.s_transponlylit_ll = TheClient.s_transponlylit_ll.Bind();
                            FBOid = FBOID.TRANSP_LIT_LL;
                        }
                        else
                        {
                            TheClient.s_transponlylit = TheClient.s_transponlylit.Bind();
                            FBOid = FBOID.TRANSP_LIT;
                        }
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponly_ll = TheClient.s_transponly_ll.Bind();
                        FBOid = FBOID.TRANSP_LL;
                    }
                    else
                    {
                        TheClient.s_transponly = TheClient.s_transponly.Bind();
                        FBOid = FBOID.TRANSP_UNLIT;
                    }
                }
                GL.UniformMatrix4(1, false, ref PrimaryMatrix_OffsetFor3D);
                CameraPos = cameraBasePos - cameraAdjust;
                RenderTransp(ref lightc, cf2);
                GL.Viewport(0, 0, Width, Height);
                CameraPos = cameraBasePos + cameraAdjust;
                CFrust = camFrust;
            }
            else
            {
                RenderTransp(ref lightc);
            }
            if (lightc == 0)
            {
                lightc = 1;
            }
            CheckError("AfterTransp");
            return lightc;
        }

        /// <summary>
        /// Apply godrays, bloom, and transparent data to screen.
        /// </summary>
        public void RenderPass_Bloom(int lightc)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
            GL.DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
            StandardBlend();
            FBOid = FBOID.NONE;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Disable(EnableCap.CullFace);
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
            GL.Disable(EnableCap.DepthTest);
            CheckError("PreGR");
            if (TheClient.CVars.r_godrays.ValueB) // TODO: Local disable? Non-primary views probably don't need godrays...
            {
                // TODO: 3d stuff for GodRays.
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.DepthTexture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, fbo_godray_texture2);
                TheClient.s_godray = TheClient.s_godray.Bind();
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(6, MainEXP);
                GL.Uniform1(7, Width / (float)Height);
                if (SunLoc.IsNaN())
                {
                    GL.Uniform2(8, new Vector2(-10f, -10f));
                }
                else
                {
                    Vector4d v = Vector4d.Transform(new Vector4d(ClientUtilities.ConvertD(SunLoc), 1.0), PrimaryMatrixd);
                    if (v.Z / v.W > 1.0f || v.Z / v.W < 0.0f)
                    {
                        GL.Uniform2(8, new Vector2(-10f, -10f));
                    }
                    else
                    {
                        Vector2d lp1 = (v.Xy / v.W) * 0.5f + new Vector2d(0.5f);
                        GL.Uniform2(8, new Vector2((float)lp1.X, (float)lp1.Y));
                        float lplenadj = (float)((1.0 - Math.Min(lp1.Length, 1.0)) * (0.99 - 0.6) + 0.6);
                        GL.Uniform1(12, 0.84f * lplenadj);
                    }
                }
                GL.Uniform1(14, TheClient.CVars.r_znear.ValueF);
                GL.Uniform1(15, TheClient.CVars.r_zfar.ValueF);
                GL.Uniform1(16, TheClient.dist); // TODO: Local controlled variable.
                TranspBlend();
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                StandardBlend();
            }
            CheckError("PostGR");
            {
                // TODO: Merge transp-to-screen and GR pass?
                //GL.Enable(EnableCap.DepthTest);
                GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
                TheClient.s_transpadder = TheClient.s_transpadder.Bind();
                GL.UniformMatrix4(1, false, ref SimpleOrthoMatrix);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(3, (float)lightc);
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            GL.UseProgram(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.DepthMask(true);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            CheckError("WrapUp");
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }
        
        /// <summary>
        /// Render transparent objects into a temporary buffer.
        /// </summary>
        void RenderTransp(ref int lightc, Frustum frustumToUse = null)
        {
            if (TheClient.CVars.r_transpll.ValueB)
            {
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, TransTexs[0]);
                GL.BindImageTexture(4, TransTexs[0], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture5);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[1]);
                GL.BindImageTexture(5, TransTexs[1], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[2]);
                GL.BindImageTexture(6, TransTexs[2], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.TextureBuffer, TransTexs[3]);
                GL.BindImageTexture(7, TransTexs[3], 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.R32ui);
                GL.ActiveTexture(TextureUnit.Texture0);
                TheClient.s_ll_clearer.Bind();
                GL.Uniform2(4, new Vector2(Width, Height));
                Matrix4 flatProj = Matrix4.CreateOrthographicOffCenter(-1, 1, 1, -1, -1, 1);
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
                //s_whatever.Bind();
                //GL.Uniform2(4, new Vector2(Window.Width, Window.Height));
                //GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 1f });
                //GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1f });
                //Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(FOV, Window.Width / (float)Window.Height, ZNear, ZFar);
                //Matrix4 view = Matrix4.LookAt(CamPos, CamGoal, Vector3.UnitZ);
                //Matrix4 combined = view * proj;
                //GL.UniformMatrix4(1, false, ref combined);
                renderTranspInt(ref lightc, frustumToUse);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
                TheClient.s_ll_fpass.Bind();
                GL.Uniform2(4, new Vector2(Width, Height));
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(4, new Vector2(Width, Height));
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            else
            {
                renderTranspInt(ref lightc, frustumToUse);
            }
        }

        public bool RenderLights = false;

        void renderTranspInt(ref int lightc, Frustum frustumToUse)
        {
            if (frustumToUse == null)
            {
                frustumToUse = camFrust;
            }
            if (TheClient.CVars.r_transplighting.ValueB)
            {
                RenderLights = true;
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            lightc++;
                        }
                    }
                }
                int c = 0;
                float[] l_dats1 = new float[LIGHTS_MAX * 16];
                float[] l_dats2 = new float[LIGHTS_MAX * 16];
                float[] s_mats = new float[LIGHTS_MAX * 16];
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || frustumToUse == null || frustumToUse.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            Matrix4 lmat = Lights[i].InternalLights[x].GetMatrix();
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
                            matxyz[2, 1] = 1f / TheClient.CVars.r_shadowquality.ValueI;
                            matxyz[2, 2] = 0.5f;
                            matxyz[2, 3] = (float)lightc;
                            matxyz[3, 0] = (float)ambient.X;
                            matxyz[3, 1] = (float)ambient.Y;
                            matxyz[3, 2] = (float)ambient.Z;
                            Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                            matabc[0, 0] = 0f;
                            matabc[0, 1] = 0f;
                            matabc[0, 2] = 0f;
                            matabc[1, 0] = (float)(Lights[i].EyePos.X - CameraPos.X);
                            matabc[1, 1] = (float)(Lights[i].EyePos.Y - CameraPos.Y);
                            matabc[1, 2] = (float)(Lights[i].EyePos.Z - CameraPos.Z);
                            matabc[2, 0] = MainEXP;
                            matabc[0, 3] = Lights[i].InternalLights[x].color.X;
                            matabc[2, 1] = Lights[i].InternalLights[x].color.Y;
                            matabc[2, 2] = Lights[i].InternalLights[x].color.Z;
                            for (int mx = 0; mx < 4; mx++)
                            {
                                for (int my = 0; my < 4; my++)
                                {
                                    s_mats[c * 16 + mx * 4 + my] = lmat[mx, my];
                                    l_dats1[c * 16 + mx * 4 + my] = matxyz[mx, my];
                                    l_dats2[c * 16 + mx * 4 + my] = matabc[mx, my];
                                }
                            }
                            c++;
                            if (c >= LIGHTS_MAX)
                            {
                                goto lights_apply;
                            }
                        }
                    }
                }
                lights_apply:
                CheckError("PreRenderTranspLights");
                if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlyvoxlitsh_ll = TheClient.s_transponlyvoxlitsh_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlyvoxlitsh = TheClient.s_transponlyvoxlitsh.Bind();
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlyvoxlit_ll = TheClient.s_transponlyvoxlit_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlyvoxlit = TheClient.s_transponlyvoxlit.Bind();
                    }
                }
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform1(7, (float)TheClient.GlobalTickTimeLocal);
                GL.Uniform2(8, new Vector2(Width, Height));
                GL.Uniform1(9, (float)c);
                GL.UniformMatrix4(10, LIGHTS_MAX, false, s_mats);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, l_dats1);
                GL.UniformMatrix4(30, LIGHTS_MAX, false, l_dats2);
                if (TheClient.CVars.r_transpshadows.ValueB && TheClient.CVars.r_shadows.ValueB)
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlylitsh_ll = TheClient.s_transponlylitsh_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlylitsh = TheClient.s_transponlylitsh.Bind();
                    }
                }
                else
                {
                    if (TheClient.CVars.r_transpll.ValueB)
                    {
                        TheClient.s_transponlylit_ll = TheClient.s_transponlylit_ll.Bind();
                    }
                    else
                    {
                        TheClient.s_transponlylit = TheClient.s_transponlylit.Bind();
                    }
                }
                GL.UniformMatrix4(2, false, ref IdentityMatrix);
                GL.Uniform2(8, new Vector2(Width, Height));
                GL.Uniform1(9, (float)c);
                GL.UniformMatrix4(10, LIGHTS_MAX, false, s_mats);
                GL.UniformMatrix4(20, LIGHTS_MAX, false, l_dats1);
                GL.UniformMatrix4(30, LIGHTS_MAX, false, l_dats2);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2DArray, fbo_shadow_tex);
                GL.ActiveTexture(TextureUnit.Texture0);
                Render3D(this);
                CheckError("PostRenderTranspLights");
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture0);
                RenderLights = false;
            }
            else
            {
                if (TheClient.CVars.r_transpll.ValueB)
                {
                    TheClient.s_transponlyvox_ll.Bind();
                    // GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                    Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                    matabc[0, 3] = (float)Width;
                    matabc[1, 3] = (float)Height;
                    GL.UniformMatrix4(9, false, ref matabc);
                    TheClient.s_transponly_ll.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                    GL.UniformMatrix4(9, false, ref matabc);
                }
                else
                {
                    TheClient.s_transponlyvox.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                    TheClient.s_transponly.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref IdentityMatrix);
                }
                Render3D(this);
            }
        }
    }

    public enum FBOID : byte
    {
        NONE = 0,
        MAIN = 1,
        TRANSP_UNLIT = 3,
        SHADOWS = 4,
        TRANSP_LIT = 7,
        TRANSP_SHADOWS = 8,
        TRANSP_LL = 12,
        TRANSP_LIT_LL = 13,
        TRANSP_SHADOWS_LL = 14,
        REFRACT = 21,
        FORWARD_TRANSP = 98,
        FORWARD_SOLID = 99,
    }

    public static class FBOIDExtensions
    {
        public static bool IsMainTransp(this FBOID id)
        {
            return id == FBOID.TRANSP_LIT || id == FBOID.TRANSP_LIT_LL || id == FBOID.TRANSP_LL || id == FBOID.TRANSP_SHADOWS || id == FBOID.TRANSP_SHADOWS_LL || id == FBOID.TRANSP_UNLIT;
        }
    }
}
