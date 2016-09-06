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
        public bool NeedsUpdate = true;
        public bool transp = false;

        public void Create(Vector3 pos, Vector3 targ, float fov, float max_range, Vector3 col)
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
            return Matrix4.LookAt(eye, target, up) *
                Matrix4.CreatePerspectiveFieldOfView(FOV * (float)Math.PI / 180f, 1, 0.1f, maxrange);
        }
    }
}
