using OpenTK;

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
