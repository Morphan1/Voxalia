﻿using Voxalia.Shared;
using OpenTK;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class SpotLight: LightObject
    {
        float Radius;

        Location Color;

        public Location Direction;

        float Width;

        public SpotLight(Location pos, float radius, Location col, Location dir, float size)
        {
            EyePos = pos;
            Radius = radius;
            Color = col;
            Width = size;
            InternalLights.Add(new Light());
            if (dir.Z >= 1 || dir.Z <= -1)
            {
                InternalLights[0].up = new Vector3(0, 1, 0);
            }
            else
            {
                InternalLights[0].up = new Vector3(0, 0, 1);
            }
            Direction = dir;
            InternalLights[0].Create(ClientUtilities.ConvertD(pos), ClientUtilities.ConvertD(pos + dir), Width, Radius, ClientUtilities.Convert(Color));
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
            InternalLights[0].eye = ClientUtilities.ConvertD(EyePos);
            InternalLights[0].target = ClientUtilities.ConvertD(EyePos + Direction);
        }
    }
}
