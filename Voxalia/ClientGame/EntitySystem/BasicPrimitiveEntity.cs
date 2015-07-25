using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.ParticleSystem;
using Voxalia.ClientGame.WorldSystem;

namespace Voxalia.ClientGame.EntitySystem
{
    public class BasicPrimitiveEntity: PrimitiveEntity
    {
        public BasicPrimitiveEntity(World tworld, bool cast_shadows)
            : base(tworld, cast_shadows)
        {
        }

        public Model model;

        public override void Destroy()
        {
        }

        public override void Spawn()
        {
            ppos = Position;
        }

        Location ppos = Location.Zero;

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
                TheClient.Rendering.SetColor(Color4.White);
                TheClient.Rendering.RenderCylinder(GetPosition(), GetPosition() - Velocity / 20f, 0.01f);
                TheClient.Particles.AddEffect(ParticleEffectType.CYLINDER, ppos, Position, 0.01f, 2f, Location.One, true, TheClient.Textures.GetTexture("common/smoke"));
                ppos = Position;
            }
        }
    }
}
