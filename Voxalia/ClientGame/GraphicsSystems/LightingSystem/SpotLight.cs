﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using OpenTK;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public class SpotLight: LightObject
    {
        int Texsize;

        float Radius;

        Location Color;

        public Location Direction;

        float Width;

        public SpotLight(Location pos, int tsize, float radius, Location col, Location dir, float size)
        {
            EyePos = pos;
            Texsize = tsize;
            Radius = radius;
            Color = col;
            Width = size;
            InternalLights.Add(new Light());
            InternalLights[0].Create(Texsize, pos.ToOVector(), (pos + dir).ToOVector(), Width, Radius, Color.ToOVector());
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
            InternalLights[0].eye = EyePos.ToOVector();
            InternalLights[0].target = (EyePos + Direction).ToOVector();
        }
    }
}