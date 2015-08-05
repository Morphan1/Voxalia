using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    class ModelEntity: PhysicsEntity
    {
        public Model model;

        public Location scale = Location.One;

        public string mod;

        public Matrix4 transform;

        public Location Offset;

        public ModelCollisionMode mode = ModelCollisionMode.AABB;

        public ModelEntity(string model_in, Region tregion)
            : base(tregion, true, true)
        {
            mod = model_in;
        }

        public override void Tick()
        {
            base.Tick();
        }

        public override void SpawnBody()
        {
            model = TheClient.Models.GetModel(mod);
            model.LoadSkin(TheClient.Textures);
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheClient.Models.Handler.MeshToBepu(model.OriginalModel);
            }
            else if (mode == ModelCollisionMode.AABB)
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.OriginalModel);
                Location zero = Location.FromBVector(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(Location.FromBVector(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                Shape = new BoxShape((float)size.X, (float)size.Y, (float)size.Z);
                Offset = -center;
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.OriginalModel);
                Location zero = new Location(vecs[0].X, vecs[0].Y, vecs[0].Z);
                double distSq = 0;
                for (int v = 1; v < vecs.Count; v++)
                {
                    if (vecs[v].LengthSquared() > distSq)
                    {
                        distSq = vecs[v].LengthSquared();
                    }
                }
                double size = Math.Sqrt(distSq);
                Offset = Location.Zero;
                Shape = new SphereShape((float)size);
            }
            base.SpawnBody();
            if (mode == ModelCollisionMode.PRECISE)
            {
                Offset = InternalOffset;
            }
            transform = Matrix4.CreateTranslation(Offset.ToOVector());
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
            if (model.Meshes[0].vbo.Tex == null)
            {
                TheClient.Textures.White.Bind();
            }
            model.Draw(); // TODO: Animation?
        }
    }

    public enum ModelCollisionMode : byte
    {
        PRECISE = 1,
        AABB = 2
    }
}
