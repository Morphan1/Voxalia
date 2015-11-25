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
    public class GrenadeEntity: PhysicsEntity
    {
        public Model model;

        public Color4 GColor;

        public GrenadeEntity(Region tregion, bool shadows)
            : base(tregion, true, shadows)
        {
            model = TheClient.Models.Sphere;
            GColor = new Color4(0f, 0f, 0f, 1f);
            ConvexEntityShape = new CylinderShape(0.2f, 0.05f);
            Shape = ConvexEntityShape;
            Bounciness = 0.95f;
            SetMass(1);
        }

        public override void Render()
        {
            TheClient.Textures.White.Bind();
            Matrix4 mat = Matrix4.CreateScale(0.05f, 0.2f, 0.05f) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetColor(GColor);
            model.Draw();
            TheClient.Rendering.SetColor(Color4.White);
        }
    }
}
