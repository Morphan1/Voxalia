using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ClientGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    class BlockItemEntity: PhysicsEntity
    {
        public Material Mat;

        public BlockItemEntity(World tworld, Material tmat)
            : base(tworld, false, true)
        {
            Mat = tmat;
            Shape = new BoxShape(1, 1, 1);
            SetMass(5);
        }

        //public VBO vbo;

        public override void SpawnBody()
        {
            // Create VBO
            base.SpawnBody();
        }

        public override void DestroyBody()
        {
            // Delete VBO
            base.DestroyBody();
        }

        public override void Render()
        {
            Matrix4 mat = GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Models.GetModel("cube").Draw();
        }
    }
}
