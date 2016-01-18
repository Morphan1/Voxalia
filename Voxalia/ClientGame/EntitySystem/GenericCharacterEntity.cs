using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using Voxalia.Shared;

namespace Voxalia.ClientGame.EntitySystem
{
    public class GenericCharacterEntity : CharacterEntity
    {
        public Model model;

        public Color4 color;

        public GenericCharacterEntity(Region tregion)
            : base(tregion)
        {
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            model.LoadSkin(TheClient.Textures);
        }

        public Matrix4 PreRot = Matrix4.Identity;

        public override void Render()
        {
            TheClient.Rendering.SetColor(color);
            TheClient.Rendering.SetMinimumLight(0.0f);
            Matrix4 mat = PreRot * GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            model.Draw();
            TheClient.Rendering.SetColor(Color4.White);
        }
    }
}
