//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.OtherSystems;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class Light
    {
        public Vector3d eye;
        public Vector3d target;
        public Vector3 up = Vector3.UnitZ;
        public float FOV;
        public float maxrange;
        public Vector3 color;
        public bool NeedsUpdate = true;
        public bool transp = false;

        public void Create(Vector3d pos, Vector3d targ, float fov, float max_range, Vector3 col)
        {
            eye = pos;
            target = targ;
            FOV = fov;
            maxrange = max_range;
            color = col;
        }

        public void Destroy()
        {
        }

        public void SetProj()
        {
            Matrix4 mat = GetMatrix();
            GL.UniformMatrix4(1, false, ref mat);
        }
        
        public virtual Matrix4 GetMatrix()
        {
            Vector3d c = ClientUtilities.ConvertD(Client.Central.MainWorldView.RenderRelative);
            Vector3d e = eye - c;
            Vector3d d = target - c;
            return Matrix4.LookAt(new Vector3((float)e.X, (float)e.Y, (float)e.Z), new Vector3((float)d.X, (float)d.Y, (float)d.Z), up) *
                Matrix4.CreatePerspectiveFieldOfView(FOV * (float)Math.PI / 180f, 1, 0.1f, maxrange);
        }
    }
}
