using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    class LightOrtho: Light
    {
        public override OpenTK.Matrix4 GetMatrix()
        {
            return /*Matrix4.LookAt(eye, target, up) * */Matrix4.CreateOrthographic(FOV, FOV, 0.1f, maxrange);
        }
    }
}
