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
            Assimp.Matrix4x4 mat = model.OriginalModel.RootNode.Transform;
            offset = -Location.FromBVector(Shape.Position);
            Matrix4 root = new Matrix4(mat.A1, mat.A2, mat.A3, mat.A4, mat.B1, mat.B2, mat.B3, mat.B4, mat.C1, mat.C2, mat.C3, mat.C4, mat.D1, mat.D2, mat.D3, mat.D4); // TODO: is this needed? Is this even valid?
            transform = Matrix4.CreateTranslation(offset.ToOVector()) * root;
            base.SpawnBody();
        }

        public override void Render()
        {
            Matrix4 orient = GetOrientationMatrix();
            Matrix4 mat = transform * (Matrix4.CreateScale(scale.ToOVector()) * orient * Matrix4.CreateTranslation((GetPosition()).ToOVector()));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            model.Draw();
        }
    }
}
