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
        public double ShadowSpikeTime;
        public double FBOSpikeTime;
        public double LightsSpikeTime;

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
                GL.DeleteFramebuffer(fbo_hdr);
                GL.DeleteTexture(fbo_hdr_tex);
            }
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
        }

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

        int fbo_hdr;
        int fbo_hdr_tex;
        
        int transp_fbo_main = 0;
        int transp_fbo_texture = 0;
        int transp_fbo_depthtex = 0;

        public void Generate(Client tclient, int w, int h)
        {
            TheClient = tclient;
            Width = w;
            Height = h;
            GenerateLightHelpers();
            GenerateTranspHelpers();
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
        
        public Location CameraUp = Location.UnitZ;

        public Location ambient;

        public float DesaturationAmount = 0f;
        
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

        public Location ForwardVec;

        public Matrix4 combined;

        public Matrix4 PrimaryMatrix;

        public Frustum cf2;

        public Frustum CFrust;

        public int LightsC = 0;

        public float[] ClearColor = new float[] { 0f, 1f, 1f, 0f };

        public void CheckError(string loc)
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

        public float FindExp(float[] inp) // TODO: Offload this to the GPU (computer shader probably, or layers of rendering...)
        {
            float total = 0f;
            for (int i = 0; i < inp.Length; i += 4)
            {
                total += Math.Max(Math.Max(inp[i], inp[i + 1]), inp[i + 2]);
            }
            return total / (float)(inp.Length / 4);
        }

        public void Render()
        {
            Stopwatch timer = new Stopwatch();
            try
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, CurrentFBO);
                GL.DrawBuffer(CurrentFBO == 0 ? DrawBufferMode.Back : DrawBufferMode.ColorAttachment0);
                StandardBlend();
                GL.Enable(EnableCap.DepthTest);
                RenderTextures = true;
                GL.ClearBuffer(ClearBuffer.Color, 0, ClearColor);
                GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
                Location cameraBasePos;
                Location cameraAdjust;
                cameraBasePos = CameraPos;
                cameraAdjust = -ForwardVec.CrossProduct(CameraUp) * 0.25;
                SetViewport();
                CameraTarget = CameraPos + ForwardVec;
                Matrix4 proj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(TheClient.CVars.r_fov.ValueF),
                    (float)Width / (float)Height, TheClient.CVars.r_znear.ValueF, TheClient.CVars.r_zfar.ValueF); // TODO: View3D-level vars?
                Location bx = TheClient.CVars.r_3d_enable.ValueB ? (CameraPos + cameraAdjust) : CameraPos;
                Matrix4 view = Matrix4.LookAt(ClientUtilities.Convert(bx), ClientUtilities.Convert(bx + ForwardVec), ClientUtilities.Convert(CameraUp));
                combined = view * proj;
                PrimaryMatrix = combined;
                Matrix4 view2 = Matrix4.LookAt(ClientUtilities.Convert(CameraPos - cameraAdjust), ClientUtilities.Convert(CameraPos - cameraAdjust + ForwardVec), ClientUtilities.Convert(CameraUp));
                Matrix4 combined2 = view2 * proj;
                Frustum camFrust = new Frustum(combined);
                //  cf1 = camFrust;
                cf2 = new Frustum(combined2);
                if (TheClient.CVars.r_fast.ValueB)
                {
                    RenderingShadows = false;
                    CFrust = camFrust;
                    GL.ActiveTexture(TextureUnit.Texture0);
                    Matrix4 fident = Matrix4.Identity;
                    FBOid = FBOID.FORWARD_SOLID;
                    TheClient.s_forw_vox.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref fident);
                    GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                    TheClient.Rendering.SetColor(Color4.White);
                    TheClient.s_forw.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref fident);
                    GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                    TheClient.Rendering.SetColor(Color4.White);
                    Render3D(this);
                    FBOid = FBOID.FORWARD_TRANSP;
                    TheClient.s_forw_vox_trans.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref fident);
                    GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                    TheClient.Rendering.SetColor(Color4.White);
                    TheClient.s_forw_trans.Bind();
                    GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref fident);
                    GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                    TheClient.Rendering.SetColor(Color4.White);
                    if (PostFirstRender != null)
                    {
                        PostFirstRender();
                    }
                    GL.DepthMask(false);
                    Render3D(this);
                    GL.DepthMask(true);
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                    GL.DrawBuffer(DrawBufferMode.Back);
                    return;
                }
                CheckError("AfterSetup");
                if (TheClient.shouldRedrawShadows && TheClient.CVars.r_shadows.ValueB && ShadowingAllowed)
                {
                    timer.Start();
                    TheClient.s_shadow = TheClient.s_shadow.Bind();
                    VBO.BonesIdentity();
                    RenderingShadows = true;
                    ShadowsOnly = true;
                    LightsC = 0;
                    Location campos = CameraPos;
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                        {
                            if (Lights[i] is SkyLight || (Lights[i].EyePos - CameraPos).LengthSquared() <
                                TheClient.CVars.r_lightmaxdistance.ValueD * TheClient.CVars.r_lightmaxdistance.ValueD + Lights[i].MaxDistance * Lights[i].MaxDistance * 6)
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
                                    CameraPos = ClientUtilities.Convert(Lights[i].InternalLights[x].eye);
                                    TheClient.s_shadowvox = TheClient.s_shadowvox.Bind();
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
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    FBOid = FBOID.SHADOWS;
                                    TheClient.s_shadow = TheClient.s_shadow.Bind();
                                    GL.UniformMatrix4(2, false, ref tident);
                                    if (Lights[i].InternalLights[x] is LightOrtho)
                                    {
                                        GL.Uniform1(3, 1.0f);
                                    }
                                    else
                                    {
                                        GL.Uniform1(3, 0.0f);
                                    }
                                    GL.Uniform1(4, Lights[i].InternalLights[x].transp ? 1.0f : 0.0f);
                                    Lights[i].InternalLights[x].Attach();
                                    TranspShadows = Lights[i].InternalLights[x].transp;
                                    // TODO: Render settings
                                    Render3D(this);
                                    FBOid = FBOID.NONE;
                                    Lights[i].InternalLights[x].Complete();
                                }
                            }
                        }
                    }
                    CameraPos = campos;
                    RenderingShadows = false;
                    ShadowsOnly = false;
                    timer.Stop();
                    ShadowTime = (double)timer.ElapsedMilliseconds / 1000f;
                    if (ShadowTime > ShadowSpikeTime)
                    {
                        ShadowSpikeTime = ShadowTime;
                    }
                    timer.Reset();
                    CheckError("AfterShadows");
                }
                timer.Start();
                SetViewport();
                Matrix4 matident = Matrix4.Identity;
                TheClient.s_fbov = TheClient.s_fbov.Bind();
                GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.Uniform2(8, new Vector2(TheClient.sl_min, TheClient.sl_max));
                TheClient.s_fbot = TheClient.s_fbot.Bind();
                GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                TheClient.s_fbo = TheClient.s_fbo.Bind();
                GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                FBOid = FBOID.MAIN;
                RenderingShadows = false;
                CFrust = camFrust;
                GL.ActiveTexture(TextureUnit.Texture0);
                RS4P.Bind();
                RenderLights = true;
                RenderSpecular = true;
                TheClient.Rendering.SetColor(Color4.White);
                VBO.BonesIdentity();
                StandardBlend();
                if (TheClient.CVars.r_3d_enable.ValueB)
                {
                    GL.Viewport(Width / 2, 0, Width / 2, Height);
                    Render3D(this);
                    CFrust = cf2;
                    GL.Viewport(0, 0, Width / 2, Height);
                    CameraPos = cameraBasePos - cameraAdjust;
                    TheClient.s_fbov = TheClient.s_fbov.Bind();
                    GL.UniformMatrix4(1, false, ref combined2);
                    TheClient.s_fbot = TheClient.s_fbot.Bind();
                    GL.UniformMatrix4(1, false, ref combined2);
                    TheClient.s_fbo = TheClient.s_fbo.Bind();
                    GL.UniformMatrix4(1, false, ref combined2);
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
                FBOid = FBOID.REFRACT;
                TheClient.s_fbov_refract = TheClient.s_fbov_refract.Bind();
                GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.Uniform2(8, new Vector2(TheClient.sl_min, TheClient.sl_max));
                TheClient.s_fbo_refract = TheClient.s_fbo_refract.Bind();
                GL.Uniform1(6, (float)TheClient.GlobalTickTimeLocal);
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref matident);
                GL.DepthMask(false);
                if (TheClient.CVars.r_3d_enable.ValueB)
                {
                    GL.Viewport(Width / 2, 0, Width / 2, Height);
                    Render3D(this);
                    CFrust = cf2;
                    GL.Viewport(0, 0, Width / 2, Height);
                    CameraPos = cameraBasePos - cameraAdjust;
                    TheClient.s_fbov_refract = TheClient.s_fbov_refract.Bind();
                    GL.UniformMatrix4(1, false, ref combined2);
                    TheClient.s_fbo_refract = TheClient.s_fbo_refract.Bind();
                    GL.UniformMatrix4(1, false, ref combined2);
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
                timer.Reset();
                timer.Start();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
                GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, RenderClearAlpha });
                Shader s_other;
                if (TheClient.CVars.r_shadows.ValueB)
                {
                    s_other = TheClient.s_shadowadder;
                    s_other.Bind();
                    GL.Uniform1(13, TheClient.CVars.r_shadowblur.ValueF);
                }
                else
                {
                    s_other = TheClient.s_lightadder;
                }
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.PositionTexture);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.NormalsTexture);
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
                float flare_val = 3f; // TODO: WHY???
                if (TheClient.CVars.r_lighting.ValueB)
                {
                    for (int i = 0; i < Lights.Count; i++)
                    {
                        if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                        {
                            s_other.Bind();
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
                                GL.Uniform1(11, Lights[i] is SpotLight ? 1f : 0f);
                                for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                                {
                                    if (Lights[i].InternalLights[x].color.LengthSquared <= 0.01)
                                    {
                                        continue;
                                    }
                                    if (TheClient.CVars.r_shadows.ValueB)
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
                                    if (TheClient.CVars.r_shadows.ValueB)
                                    {
                                        GL.Uniform1(12, 1f / Lights[i].InternalLights[x].texsize);
                                    }
                                    TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                                    GL.ActiveTexture(TextureUnit.Texture0);
                                    GL.BindTexture(TextureTarget.Texture2D, 0);
                                }
                            }
                        }
                    }
                    TheClient.s_applyambient = TheClient.s_applyambient.Bind();
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.RenderhintTexture);
                    GL.ActiveTexture(TextureUnit.Texture0);
                    GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                    GL.UniformMatrix4(1, false, ref mat);
                    GL.UniformMatrix4(2, false, ref matident);
                    GL.Uniform3(5, ClientUtilities.Convert(ambient));
                    TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
                    CheckError("AfterLighting");
                    if (TheClient.CVars.r_hdr.ValueB)
                    {
                        float[] rd = new float[Width * Height];
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, fbo_main);
                        GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
                        GL.ReadPixels(0, 0, Width, Height, PixelFormat.Red, PixelType.Float, rd);
                        GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
                        GL.ReadBuffer(ReadBufferMode.None);
                        float exp = FindExp(rd);
                        exp = Math.Max(Math.Min(exp, 2.0f), 0.33f);
                        exp = 1.0f / exp;
                        flare_val += exp * 4f;
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
                        CheckError("AfterHDRRead");
                    }
                    else
                    {
                        MainEXP = 1.0f;
                    }
                }
                else
                {
                    MainEXP = 1.0f;
                }
                CheckError("AfterAllLightCode");
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_godray_main);
                if (TheClient.CVars.r_toonify.ValueB)
                {
                    TheClient.s_finalgodray_toonify = TheClient.s_finalgodray_toonify.Bind();
                }
                else
                {
                    TheClient.s_finalgodray = TheClient.s_finalgodray.Bind();
                }
                GL.DrawBuffers(2, new DrawBuffersEnum[] { DrawBuffersEnum.ColorAttachment0, DrawBuffersEnum.ColorAttachment1 });
                GL.Uniform3(5, ClientUtilities.Convert(TheClient.CVars.r_lighting.ValueB ? Location.Zero : Location.One));
                GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0f, 0f, 0f, 0f });
                GL.ClearBuffer(ClearBuffer.Color, 1, new float[] { 0f, 0f, 0f, 0f });
                GL.BlendFuncSeparate(1, BlendingFactorSrc.SrcColor, BlendingFactorDest.Zero, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
                GL.Uniform1(19, DesaturationAmount);
                GL.Uniform3(8, ClientUtilities.Convert(TheClient.CameraFinalTarget));
                GL.Uniform1(9, TheClient.CVars.r_dof_strength.ValueF);
                GL.Uniform1(16, TheClient.CVars.r_znear.ValueF);
                GL.Uniform1(17, TheClient.CVars.r_zfar.ValueF);
                GL.Uniform4(18, new Vector4(ClientUtilities.Convert(Headmat.GetFogColor()), Headmat.GetFogAlpha()));
                GL.Uniform3(20, ClientUtilities.Convert(CameraPos));
                GL.Uniform1(21, TheClient.CVars.r_znear.ValueF);
                GL.Uniform1(23, TheClient.CVars.r_zfar.ValueF);
                GL.Uniform1(24, (float)Width);
                GL.Uniform1(25, (float)Height);
                GL.Uniform1(26, (float)TheClient.GlobalTickTimeLocal);
                GL.Uniform1(27, (float)MainEXP);
                GL.Uniform1(28, flare_val);
                GL.UniformMatrix4(22, false, ref combined);
                GL.ActiveTexture(TextureUnit.Texture4);
                GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
                GL.ActiveTexture(TextureUnit.Texture7);
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.ActiveTexture(TextureUnit.Texture6);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.Rh2Texture);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, RS4P.DiffuseTexture);
                GL.UniformMatrix4(1, false, ref mat);
                GL.UniformMatrix4(2, false, ref matident);
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
                Matrix4 def = Matrix4.Identity;
                GL.Enable(EnableCap.CullFace);
                CheckError("AfterFirstRender");
                if (PostFirstRender != null)
                {
                    PostFirstRender();
                }
                CheckError("AfterPostFirstRender");
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
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
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
                VBO.BonesIdentity();
                GL.UniformMatrix4(1, false, ref combined);
                GL.UniformMatrix4(2, false, ref def);
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
                if (TheClient.CVars.r_3d_enable.ValueB)
                {
                    GL.Viewport(Width / 2, 0, Width / 2, Height);
                    CameraPos = cameraBasePos + cameraAdjust;
                    RenderTransp(ref lightc, camFrust);
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
                    GL.UniformMatrix4(1, false, ref combined2);
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
                    GL.UniformMatrix4(1, false, ref combined2);
                    CameraPos = cameraBasePos - cameraAdjust;
                    RenderTransp(ref lightc, cf2);
                    GL.Viewport(0, 0, Width, Height);
                    CameraPos = cameraBasePos + cameraAdjust;
                    CFrust = camFrust;
                }
                else
                {
                    RenderTransp(ref lightc, camFrust);
                }
                if (lightc == 0)
                {
                    lightc = 1;
                }
                CheckError("AfterTransp");
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
                    GL.UniformMatrix4(1, false, ref mat);
                    GL.UniformMatrix4(2, false, ref matident);
                    GL.Uniform1(6, MainEXP);
                    GL.Uniform1(7, Width / (float)Height);
                    if (SunLoc.IsNaN())
                    {
                        GL.Uniform2(8, new Vector2(-10f, -10f));
                    }
                    else
                    {
                        Vector4 v = Vector4.Transform(new Vector4(ClientUtilities.Convert(SunLoc), 1f), combined);
                        if (v.Z / v.W > 1.0f || v.Z / v.W < 0.0f)
                        {
                            GL.Uniform2(8, new Vector2(-10f, -10f));
                        }
                        else
                        {
                            Vector2 lp1 = (v.Xy / v.W) * 0.5f + new Vector2(0.5f);
                            GL.Uniform2(8, ref lp1);
                            float lplenadj = (1f - Math.Min(lp1.Length, 1f)) * (0.99f - 0.6f) + 0.6f;
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
                    //GL.Enable(EnableCap.DepthTest);
                    GL.BindTexture(TextureTarget.Texture2D, transp_fbo_texture);
                    TheClient.s_transpadder = TheClient.s_transpadder.Bind();
                    GL.UniformMatrix4(1, false, ref mat);
                    GL.UniformMatrix4(2, false, ref matident);
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
                timer.Stop();
                LightsTime = (double)timer.ElapsedMilliseconds / 1000f;
                if (LightsTime > LightsSpikeTime)
                {
                    LightsSpikeTime = LightsTime;
                }
                timer.Reset();
                CheckError("AtEnd");
            }
            catch (Exception ex)
            {
                SysConsole.Output("Rendering (3D)", ex);
            }
        }

        void RenderTransp(ref int lightc, Frustum camFrust)
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
                Matrix4 ident = Matrix4.Identity;
                GL.UniformMatrix4(2, false, ref ident);
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
                renderTranspInt(ref lightc, camFrust);
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
                TheClient.s_ll_fpass.Bind();
                GL.Uniform2(4, new Vector2(Width, Height));
                GL.UniformMatrix4(1, false, ref flatProj);
                GL.UniformMatrix4(2, false, ref ident);
                GL.Uniform2(4, new Vector2(Width, Height));
                TheClient.Rendering.RenderRectangle(-1, -1, 1, 1);
            }
            else
            {
                renderTranspInt(ref lightc, camFrust);
            }
        }

        public bool RenderLights = false;

        void renderTranspInt(ref int lightc, Frustum camFrust)
        {
            if (TheClient.CVars.r_transplighting.ValueB)
            {
                RenderLights = true;
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
                        {
                            lightc++;
                        }
                    }
                }
                for (int i = 0; i < Lights.Count; i++)
                {
                    if (Lights[i] is SkyLight || camFrust == null || camFrust.ContainsSphere(Lights[i].EyePos.ToBVector(), Lights[i].MaxDistance))
                    {
                        for (int x = 0; x < Lights[i].InternalLights.Count; x++)
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
                                GL.ActiveTexture(TextureUnit.Texture2);
                                GL.BindTexture(TextureTarget.Texture2D, Lights[i].InternalLights[x].fbo_depthtex);
                                GL.ActiveTexture(TextureUnit.Texture0);
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
                            CheckError("PreRenderATranspLight");
                            Matrix4 ident = Matrix4.Identity;
                            //  GL.UniformMatrix4(1, false, ref combined);
                            GL.UniformMatrix4(2, false, ref ident);
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
                            CheckError("HalfRenderATranspLight");
                            GL.UniformMatrix4(8, false, ref matxyz);
                            Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                            matabc[0, 0] = (float)CameraPos.X;
                            matabc[0, 1] = (float)CameraPos.Y;
                            matabc[0, 2] = (float)CameraPos.Z;
                            matabc[0, 3] = (float)Width;
                            matabc[1, 0] = (float)Lights[i].EyePos.X;
                            matabc[1, 1] = (float)Lights[i].EyePos.Y;
                            matabc[1, 2] = (float)Lights[i].EyePos.Z;
                            matabc[1, 3] = (float)Height;
                            CheckError("MidRenderTranspDetails1");
                            GL.UniformMatrix4(9, false, ref matabc);
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
                            CheckError("MidRenderTranspDetails2");
                            //GL.UniformMatrix4(1, false, ref combined);
                            GL.UniformMatrix4(2, false, ref ident);
                            CheckError("MidRenderTranspDetails2.5");
                            //GL.Uniform3(5, Lights[i].InternalLights[x].eye);
                            CheckError("MidRenderTranspDetails3");
                            GL.UniformMatrix4(6, false, ref lmat);
                            GL.Uniform3(7, Lights[i].InternalLights[x].color);
                            CheckError("MidRenderTranspDetails4");
                            GL.UniformMatrix4(8, false, ref matxyz);
                            CheckError("MidRenderTranspDetails5");
                            GL.UniformMatrix4(9, false, ref matabc);
                            CheckError("PreRenderTranspWorld");
                            Render3D(this);
                            CheckError("PostRenderATranspLight");
                        }
                    }
                }
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
                    Matrix4 ident = Matrix4.Identity;
                    // GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref ident);
                    Matrix4 matabc = new Matrix4(Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
                    matabc[0, 3] = (float)Width;
                    matabc[1, 3] = (float)Height;
                    GL.UniformMatrix4(9, false, ref matabc);
                    TheClient.s_transponly_ll.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref ident);
                    GL.UniformMatrix4(9, false, ref matabc);
                }
                else
                {
                    TheClient.s_transponlyvox.Bind();
                    Matrix4 ident = Matrix4.Identity;
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref ident);
                    TheClient.s_transponly.Bind();
                    //GL.UniformMatrix4(1, false, ref combined);
                    GL.UniformMatrix4(2, false, ref ident);
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
}
