using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.ClientMainSystem;
using ShadowOperations.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using ShadowOperations.ClientGame.GraphicsSystems;

namespace ShadowOperations.ClientGame.EntitySystem
{
    public class BasicPrimitiveEntity: PrimitiveEntity
    {
        public BasicPrimitiveEntity(Client tclient, bool cast_shadows)
            : base(tclient, cast_shadows)
        {
        }

        public Model model;

        public override void Destroy()
        {
        }

        public override void Spawn()
        {
        }

        public override void Render()
        {
            if (TheClient.RenderTextures)
            {
                TheClient.Textures.White.Bind();
            }
            if (model != null)
            {
                Matrix4 mat = Matrix4.CreateRotationX((float)(Angle.X * Utilities.PI180))
                    * Matrix4.CreateRotationY((float)(Angle.Y * Utilities.PI180))
                    * Matrix4.CreateRotationZ((float)(Angle.Z * Utilities.PI180))
                    * Matrix4.CreateTranslation(GetPosition().ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                model.Draw(); // TODO: Animation?
            }
            else
            {
                TheClient.Rendering.RenderLine(GetPosition(), GetPosition() - Velocity / 10f);
            }
        }
    }
}
