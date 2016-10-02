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
        public void Setup(Vector3d pos, Vector3d targ, float fov, float max_range, Vector3 col)
        {
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
        }
    }
}
