using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.ClientGame.ClientMainSystem;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class MarkerEntity: PrimitiveEntity
    {
        public MarkerEntity(Client tclient)
            : base(tclient)
        {
        }

        public override void Spawn()
        {
        }

        public override void Destroy()
        {
        }

        public override void Render()
        {
            Matrix4 mat = Matrix4.CreateTranslation(GetPosition().ToOVector());
            GL.UniformMatrix4(2, false, ref mat);
            TheClient.Models.Cube.Draw();
        }
    }
}
