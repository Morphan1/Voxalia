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
        public int Texsize;

        float Radius;

        Location Color;

        public PointLight(Location pos, int tsize, float radius, Location col)
        {
            EyePos = pos;
            Texsize = tsize;
            Radius = radius;
            Color = col;
            for (int i = 0; i < 6; i++)
            {
                Light li = new Light();
                li.Create(ClientUtilities.Convert(pos), ClientUtilities.Convert(pos + Location.UnitX), 90f, Radius, ClientUtilities.Convert(Color));
                InternalLights.Add(li);
            }
            InternalLights[4].up = new Vector3(0, 1, 0);
            InternalLights[5].up = new Vector3(0, 1, 0);
            Reposition(EyePos);
            MaxDistance = radius;
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Destroy()
        {
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
