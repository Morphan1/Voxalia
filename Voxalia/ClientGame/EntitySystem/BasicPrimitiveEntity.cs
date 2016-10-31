//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using Voxalia.Shared;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.ClientGame.GraphicsSystems.ParticleSystem;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    public class BasicPrimitiveEntity: PrimitiveEntity
    {
        public BasicPrimitiveEntity(Region tregion, bool cast_shadows)
            : base(tregion, cast_shadows)
        {
        }

        public Model model;

        public override void Destroy()
        {
        }

        public override void Tick()
        {
            if (model == null)
            {
                Location tpp = ppos;
                Location tp = Position;
                ParticleEffect pe = TheClient.Particles.Engine.AddEffect(ParticleEffectType.CYLINDER, (o) => tpp, (o) => tp, (o) => 0.03f, 1f, Location.One, Location.One, true, TheClient.Textures.White, 0.75f);
                pe.MinimumLight = 1f;
                pe.BlowsInWind = false;
                ppos = Position;
            }
            else if (model.Name == "projectiles/arrow.dae") // TODO: More dynamic option for this
            {
                float offs = 0.1f;
                BEPUutilities.Vector3 offz;
                BEPUutilities.Quaternion.TransformZ(offs, ref Angles, out offz);
                BEPUutilities.Vector3 offx;
                BEPUutilities.Quaternion.TransformX(offs, ref Angles, out offx);
                Location tpp = ppos;
                Location tp = Position;
                TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => tpp + new Location(offz),
                    (o) => tp + new Location(offz), (o) => 1f, 1f, Location.One,
                    Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => tpp - new Location(offz),
                    (o) => tp - new Location(offz), (o) => 1f, 1f, Location.One,
                    Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => tpp + new Location(offx),
                    (o) => tp + new Location(offx), (o) => 1f, 1f, Location.One,
                    Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                TheClient.Particles.Engine.AddEffect(ParticleEffectType.LINE, (o) => tpp - new Location(offx),
                    (o) => tp - new Location(offx), (o) => 1f, 1f, Location.One,
                    Location.One, true, TheClient.Textures.GetTexture("common/smoke"), 0.5f);
                ppos = Position;
            }
            base.Tick();
        }

        public override void Spawn()
        {
            ppos = Position;
        }

        Location ppos = Location.Zero;

        public override void Render()
        {
            if (model == null)
            {
                return;
            }
            TheClient.SetEnts();
            if (TheClient.RenderTextures)
            {
                TheClient.Textures.White.Bind();
            }
            TheClient.Rendering.SetMinimumLight(0f);
            BEPUutilities.Matrix matang = BEPUutilities.Matrix.CreateFromQuaternion(Angles);
            //matang.Transpose();
            Matrix4d matang4 = new Matrix4d(matang.M11, matang.M12, matang.M13, matang.M14,
                matang.M21, matang.M22, matang.M23, matang.M24,
                matang.M31, matang.M32, matang.M33, matang.M34,
                matang.M41, matang.M42, matang.M43, matang.M44);
            Matrix4d mat = matang4 * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(GetPosition()));
            TheClient.MainWorldView.SetMatrix(2, mat);
            model.Draw(); // TODO: Animation?
        }
    }

    public class BulletEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] e)
        {
            if (e.Length < 4 + 24 + 24)
            {
                return null;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(tregion, false);
            bpe.Scale = new Location(Utilities.BytesToFloat(Utilities.BytesPartial(e, 0, 4)));
            bpe.SetPosition(Location.FromDoubleBytes(e, 4));
            bpe.SetVelocity(Location.FromDoubleBytes(e, 4 + 24));
            return bpe;
        }
    }

    public class PrimitiveEntityConstructor : EntityTypeConstructor
    {
        public override Entity Create(Region tregion, byte[] e)
        {
            if (e.Length < 4 + 24 + 24 + 16 + 24 + 4)
            {
                return null;
            }
            BasicPrimitiveEntity bpe = new BasicPrimitiveEntity(tregion, false);
            bpe.Position = Location.FromDoubleBytes(e, 0);
            bpe.Velocity = Location.FromDoubleBytes(e, 24);
            bpe.Angles = Utilities.BytesToQuaternion(e, 24 + 24);
            bpe.Scale = Location.FromDoubleBytes(e, 24 + 24 + 16);
            bpe.Gravity = Location.FromDoubleBytes(e, 24 + 24 + 16 + 24);
            bpe.model = tregion.TheClient.Models.GetModel(tregion.TheClient.Network.Strings.StringForIndex(Utilities.BytesToInt(Utilities.BytesPartial(e, 24 + 24 + 16 + 24 + 24, 4))));
            return bpe;
        }
    }
}
