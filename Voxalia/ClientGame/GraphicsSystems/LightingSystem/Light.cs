using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class Light
    {
        public Vector3 eye;
        public Vector3 target;
        public Vector3 up = Vector3.UnitZ;
        public float FOV;
        public float maxrange;
        public Vector3 color;
        public int texsize;
        public int fbo_main;
        public int fbo_texture;
        public int fbo_depthtex;
        public bool NeedsUpdate = true;

        public void Create(int tsize, Vector3 pos, Vector3 targ, float fov, float max_range, Vector3 col)
        {
            texsize = tsize;
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
            // FBO
            fbo_main = GL.GenFramebuffer();
            // Build the texture
            fbo_texture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_texture);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R32f, texsize, texsize, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            // Attach it to the FBO
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, fbo_texture, 0);
            // Build the depth texture
            fbo_depthtex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fbo_depthtex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent32, texsize, texsize, 0,
                PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Lequal);
            // Attach it to the FBO
            // TODO: 2D->Layer, bind to a massive shadow tex array for quicker rendering!
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, fbo_depthtex, 0);
            // Wrap up
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Destroy()
        {
            GL.DeleteFramebuffer(fbo_main);
            GL.DeleteTexture(fbo_texture);
            GL.DeleteTexture(fbo_depthtex);
        }

        public virtual void Attach()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.Viewport(0, 0, texsize, texsize);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
            Client.Central.vpw = texsize; // TODO: pass client reference!
            Client.Central.vph = texsize; // TODO: pass client reference!
            SetProj();
            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
        }

        public void SetProj()
        {
            Matrix4 mat = GetMatrix();
            GL.UniformMatrix4(1, false, ref mat);
        }

        public virtual void Complete()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }

        public virtual Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(eye, target, up) *
                Matrix4.CreatePerspectiveFieldOfView(FOV * (float)Math.PI / 180f, 1, 0.1f, maxrange);
        }
    }
}
