using System;
using Voxalia.Shared;
using Voxalia.ClientGame.OtherSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class PointLight : LightObject
    {
        int Texsize;

        float Radius;

        Location Color;

        public int FBO;

        public int FBODepthTex;

        public int FBODTex;

        public PointLight(Location pos, int tsize, float radius, Location col)
        {
            EyePos = pos;
            Texsize = tsize;
            Radius = radius;
            Color = col;
            FBO = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            FBODepthTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2DArray, FBODepthTex);
            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.R32f, tsize, tsize, 6, 0, PixelFormat.Red, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Lequal);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, FBODepthTex, 0, 0);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment1, FBODepthTex, 0, 1);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment2, FBODepthTex, 0, 2);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment3, FBODepthTex, 0, 3);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment4, FBODepthTex, 0, 4);
            GL.FramebufferTextureLayer(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment5, FBODepthTex, 0, 5);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
            FBODTex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, FBODTex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent, tsize, tsize, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureCompareMode, (int)TextureCompareMode.CompareRefToTexture);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureCompareFunc, (int)DepthFunction.Lequal);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, FBODTex, 0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            for (int i = 0; i < 6; i++)
            {
                /*LightPoint lp = new LightPoint();
                lp.fbo_main = FBO;
                lp.fbo_texture = FBODepthTex;
                lp.fbo_depthtex = FBODTex;
                InternalLights.Add(lp);
                lp.Setup(Texsize, ClientUtilities.Convert(pos), ClientUtilities.Convert(pos + Location.UnitX), 90f, Radius, ClientUtilities.Convert(Color));*/
                Light li = new Light();
                li.Create(tsize, ClientUtilities.Convert(pos), ClientUtilities.Convert(pos + Location.UnitX), 90f, Radius, ClientUtilities.Convert(Color));
                InternalLights.Add(li);
            }
            /*((LightPoint)InternalLights[0]).DBM = DrawBufferMode.ColorAttachment0;
            ((LightPoint)InternalLights[1]).DBM = DrawBufferMode.ColorAttachment1;
            ((LightPoint)InternalLights[2]).DBM = DrawBufferMode.ColorAttachment2;
            ((LightPoint)InternalLights[3]).DBM = DrawBufferMode.ColorAttachment3;
            ((LightPoint)InternalLights[4]).DBM = DrawBufferMode.ColorAttachment4;
            ((LightPoint)InternalLights[5]).DBM = DrawBufferMode.ColorAttachment5;*/
            InternalLights[4].up = new Vector3(0, 1, 0);
            InternalLights[5].up = new Vector3(0, 1, 0);
            Reposition(EyePos);
            MaxDistance = radius;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Destroy()
        {
            GL.DeleteFramebuffer(FBO);
            GL.DeleteTexture(FBODepthTex);
            GL.DeleteTexture(FBODTex);
        }

        public override void Reposition(Location pos)
        {
            EyePos = pos;
            for (int i = 0; i < 6; i++)
            {
                InternalLights[i].NeedsUpdate = true;
                InternalLights[i].eye = ClientUtilities.Convert(EyePos);
            }
            InternalLights[0].target = ClientUtilities.Convert(EyePos + new Location(1, 0, 0));
            InternalLights[1].target = ClientUtilities.Convert(EyePos + new Location(-1, 0, 0));
            InternalLights[2].target = ClientUtilities.Convert(EyePos + new Location(0, 1, 0));
            InternalLights[3].target = ClientUtilities.Convert(EyePos + new Location(0, -1, 0));
            InternalLights[4].target = ClientUtilities.Convert(EyePos + new Location(0, 0, 1));
            InternalLights[5].target = ClientUtilities.Convert(EyePos + new Location(0, 0, -1));
        }
    }
}
