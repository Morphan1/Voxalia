using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    class LightOrtho: Light
    {
        public override OpenTK.Matrix4 GetMatrix()
        {
            return Matrix4.LookAt(eye, target, up) * Matrix4.CreateOrthographic(FOV, FOV, 1f, maxrange);
        }
    }
}
