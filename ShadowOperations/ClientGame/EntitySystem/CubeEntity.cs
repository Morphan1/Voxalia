using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using BulletSharp;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class CubeEntity: Entity
    {
        public CubeEntity(Client tclient, Location halfsize)
            : base(tclient, false)
        {
            HalfSize = halfsize;
            Shape = new BoxShape(HalfSize.ToBVector());
        }

        /// <summary>
        /// Half the size of the cuboid.
        /// </summary>
        public Location HalfSize = new Location(1, 1, 1);

        public override void Render()
        {
            Matrix4 mat = Matrix4.CreateScale((float)HalfSize.X, (float)HalfSize.Y, (float)HalfSize.Z) * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
        }
    }
}
