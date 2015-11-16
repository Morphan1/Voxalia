using System;
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

namespace Voxalia.ClientGame.EntitySystem
{
    class GlowstickEntity: PhysicsEntity
    {
        Model model;
        Color4 GColor;
        PointLight light;

        public GlowstickEntity(Region tregion, int color)
            : base(tregion, true, false)
        {
            model = TheClient.Models.Sphere;
            System.Drawing.Color col = System.Drawing.Color.FromArgb(color);
            GColor = new Color4(col.R, col.G, col.B, col.A);
            ConvexEntityShape = new CylinderShape(0.2f, 0.05f);
            Shape = ConvexEntityShape;
            Bounciness = 0.95f;
            SetMass(1);
        }

        public override void Render()
        {
            Matrix4 mat = Matrix4.CreateScale(0.05f, 0.2f, 0.05f) * GetTransformationMatrix();// TODO: * Matrix4.CreateRotationX(90f * (float)Utilities.PI180);
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetColor(GColor);
            TheClient.Rendering.SetMinimumLight(1);
            model.Draw();
            TheClient.Rendering.SetMinimumLight(0);
            TheClient.Rendering.SetColor(Color4.White);
        }

        public override void Tick()
        {
            light.Reposition(GetPosition());
            base.Tick();
        }

        public override void SpawnBody()
        {
            light = new PointLight(GetPosition(), 64, 6, new Location(GColor.R, GColor.G, GColor.B));
            TheClient.Lights.Add(light);
            base.SpawnBody();
        }

        public override void DestroyBody()
        {
            TheClient.Lights.Remove(light);
            light.Destroy();
            base.DestroyBody();
        }
    }
}
