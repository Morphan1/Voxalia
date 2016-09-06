using Voxalia.Shared;
using OpenTK;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class SkyLight: LightObject
    {
        float Radius;

        Location Color;

        public Location Direction;

        float Width;

        public SkyLight(Location pos, float radius, Location col, Location dir, float size, bool transp)
        {
            EyePos = pos;
            Radius = radius;
            Color = col;
            Width = size;
            InternalLights.Add(new LightOrtho());
            if (dir.Z >= 1 || dir.Z <= -1)
            {
                InternalLights[0].up = new Vector3(0, 1, 0);
            }
            else
            {
                InternalLights[0].up = new Vector3(0, 0, 1);
            }
            InternalLights[0].transp = transp;
            Direction = dir;
            InternalLights[0].Create(ClientUtilities.Convert(pos), ClientUtilities.Convert(pos + dir), Width, Radius, ClientUtilities.Convert(Color));
            MaxDistance = radius;
        }

        public void Destroy()
        {
            InternalLights[0].Destroy();
        }

        public override void Reposition(Location pos)
        {
            EyePos = pos;
            InternalLights[0].NeedsUpdate = true;
            InternalLights[0].eye = ClientUtilities.Convert(EyePos);
            InternalLights[0].target = ClientUtilities.Convert(EyePos + Direction);
        }
    }
}
