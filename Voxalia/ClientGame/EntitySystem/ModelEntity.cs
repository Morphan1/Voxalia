using System;
using System.Collections.Generic;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;
using BEPUutilities;

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

        public BEPUutilities.Vector3 ModelMin;
        public BEPUutilities.Vector3 ModelMax;

        public ModelEntity(string model_in, Region tregion)
            : base(tregion, true, true)
        {
            mod = model_in;
        }

        public override void Tick()
        {
            if (Body == null)
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            base.Tick();
        }

        public override void SpawnBody()
        {
            model = TheClient.Models.GetModel(mod);
            if (model == null || model.Original == null) // TODO: smod should return a cube when all else fails?
            {
                // TODO: Make it safe to -> TheRegion.DespawnEntity(this);
                return;
            }
            model.LoadSkin(TheClient.Textures);
            int ignoreme;
            if (mode == ModelCollisionMode.PRECISE)
            {
                Shape = TheClient.Models.Handler.MeshToBepu(model.Original, out ignoreme);
            }
            else if (mode == ModelCollisionMode.CONVEXHULL)
            {
                Shape = TheClient.Models.Handler.MeshToBepuConvex(model.Original, out ignoreme);
            }
            else if (mode == ModelCollisionMode.AABB)
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.Original);
                Location zero = new Location(vecs[0]);
                AABB abox = new AABB() { Min = zero, Max = zero };
                for (int v = 1; v < vecs.Count; v++)
                {
                    abox.Include(new Location(vecs[v]));
                }
                Location size = abox.Max - abox.Min;
                Location center = abox.Max - size / 2;
                Shape = new BoxShape((float)size.X * (float)scale.X, (float)size.Y * (float)scale.Y, (float)size.Z * (float)scale.Z);
                Offset = -center;
            }
            else
            {
                List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.Original);
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
                Shape = new SphereShape((float)size * (float)scale.X);
            }
            base.SpawnBody();
            if (mode == ModelCollisionMode.PRECISE)
            {
                Offset = InternalOffset;
            }
            BEPUutilities.Vector3 offs = Offset.ToBVector();
            transform = Matrix4.CreateTranslation(ClientUtilities.Convert(Offset));
            List<BEPUutilities.Vector3> tvecs = TheClient.Models.Handler.GetVertices(model.Original);
            ModelMin = tvecs[0];
            ModelMax = tvecs[0];
            foreach (BEPUutilities.Vector3 vec in tvecs)
            {
                BEPUutilities.Vector3 tvec = vec + offs;
                if (tvec.X < ModelMin.X) { ModelMin.X = tvec.X; }
                if (tvec.Y < ModelMin.Y) { ModelMin.Y = tvec.Y; }
                if (tvec.Z < ModelMin.Z) { ModelMin.Z = tvec.Z; }
                if (tvec.X > ModelMax.X) { ModelMax.X = tvec.X; }
                if (tvec.Y > ModelMax.Y) { ModelMax.Y = tvec.Y; }
                if (tvec.Z > ModelMax.Z) { ModelMax.Z = tvec.Z; }
            }
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            TheClient.SetEnts();
            // TODO: If farther away than a given distance (set by server), render as a sprite instead of a full model!
            RigidTransform rt = new RigidTransform(Body.Position, Body.Orientation);
            BEPUutilities.Vector3 bmin;
            BEPUutilities.Vector3 bmax;
            RigidTransform.Transform(ref ModelMin, ref rt, out bmin);
            RigidTransform.Transform(ref ModelMax, ref rt, out bmax);
            if (TheClient.CFrust != null && !TheClient.CFrust.ContainsBox(new Location(bmin), new Location(bmax)))
            {
                return;
            }
            Matrix4 orient = GetOrientationMatrix();
            Matrix4 mat = transform * (Matrix4.CreateScale(ClientUtilities.Convert(scale)) * orient * Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition())));
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            if (model.Meshes[0].vbo.Tex == null)
            {
                TheClient.Textures.White.Bind();
            }
            model.Draw(); // TODO: Animation?
        }
    }
}
