using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace ShadowOperations.ClientGame.ClientMainSystem
{
    public partial class Client
    {
        void Window_RenderFrame(object sender, FrameEventArgs e)
        {
            Window.SwapBuffers();
        }
    }
}
