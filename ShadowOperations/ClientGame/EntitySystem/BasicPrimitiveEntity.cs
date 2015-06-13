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
                TheClient.Rendering.SetMinimumLight(0f);
                BEPUutilities.Matrix matang = BEPUutilities.Matrix.CreateFromQuaternion(Angles);
                //matang.Transpose();
                Matrix4 matang4 = new Matrix4(matang.M11, matang.M12, matang.M13, matang.M14, matang.M21, matang.M22, matang.M23, matang.M24,
                    matang.M31, matang.M32, matang.M33, matang.M34, matang.M41, matang.M42, matang.M43, matang.M44);
                Matrix4 mat = matang4 * Matrix4.CreateTranslation(GetPosition().ToOVector());
                GL.UniformMatrix4(2, false, ref mat);
                model.Draw(); // TODO: Animation?
            }
            else
            {
                TheClient.Rendering.SetMinimumLight(1f);
                TheClient.Rendering.SetColor(Color4.Red);
                TheClient.Rendering.RenderCylinder(GetPosition(), GetPosition() - Velocity / 10f, 0.2f);
                TheClient.Rendering.SetColor(Color4.White);
            }
        }
    }
}
