﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.LightingSystem;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.OtherSystems;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.Shared;
using Voxalia.ClientGame.ClientMainSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    class GlowstickEntity: GrenadeEntity
    {
        PointLight light;

        public GlowstickEntity(Region tregion, int color)
            : base(tregion, false)
        {
            System.Drawing.Color col = System.Drawing.Color.FromArgb(color);
            GColor = new Color4(col.R, col.G, col.B, col.A);
        }

        public override void Render()
        {
            if (TheClient.MainWorldView.FBOid == FBOID.MAIN)
            {
                GL.Uniform4(7, new Vector4(GColor.R, GColor.B, GColor.B, 1f));
            }
            TheClient.Rendering.SetMinimumLight(1);
            base.Render();
            TheClient.Rendering.SetMinimumLight(0);
            if (TheClient.MainWorldView.FBOid == FBOID.MAIN)
            {
                GL.Uniform4(7, new Vector4(0f, 0f, 0f, 0f));
            }
        }

        public override void Tick()
        {
            light.Reposition(GetPosition());
            base.Tick();
        }

        public override void SpawnBody()
        {
            light = new PointLight(GetPosition(), 64, 15, new Location(GColor.R, GColor.G, GColor.B));
            TheClient.MainWorldView.Lights.Add(light);
            base.SpawnBody();
        }

        public override void DestroyBody()
        {
            TheClient.MainWorldView.Lights.Remove(light);
            light.Destroy();
            base.DestroyBody();
        }
    }
}
