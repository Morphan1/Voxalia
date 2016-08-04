using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    class LightPoint: Light
    {
        public DrawBufferMode DBM = DrawBufferMode.ColorAttachment0;

        public void Setup(int tsize, Vector3 pos, Vector3 targ, float fov, float max_range, Vector3 col)
        {
            texsize = tsize;
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
        }
        
        public override void Attach()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo_main);
            GL.Viewport(0, 0, texsize, texsize);
            SetProj();
            GL.DrawBuffer(DBM);
            GL.ClearBuffer(ClearBuffer.Color, 0, new float[] { 1.0f });
            GL.ClearBuffer(ClearBuffer.Depth, 0, new float[] { 1.0f });
        }
        
        public override void Complete()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }
    }
}
