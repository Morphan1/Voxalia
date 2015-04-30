using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.ClientGame.GraphicsSystems;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace ShadowOperations.ClientGame.EntitySystem
{
    class ModelEntity: PhysicsEntity
    {
        public Model model;

        public Location scale = Location.One;

        public Location offset;

        public string mod;

        public Matrix4 transform;

        public ModelEntity(string model_in, Client tclient)
            : base(tclient, false, true)
        {
            mod = model_in;
        }

        public override void SpawnBody()
        {
            model = TheClient.Models.GetModel(mod);
            Shape = TheClient.Models.Handler.MeshToBepu(model.OriginalModel);
            offset = -Location.FromBVector(Shape.Position);
            transform = Matrix4.CreateTranslation(offset.ToOVector());
            base.SpawnBody();
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            Matrix4 orient = GetOrientationMatrix();
            Matrix4 mat = transform * (Matrix4.CreateScale(scale.ToOVector()) * orient * Matrix4.CreateTranslation((GetPosition()).ToOVector()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.Draw(0);
        }
    }
}
