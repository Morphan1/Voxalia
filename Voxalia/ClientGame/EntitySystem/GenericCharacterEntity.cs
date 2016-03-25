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
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.EntitySystem
{
    public class GenericCharacterEntity : CharacterEntity
    {
        public Model model;

        public System.Drawing.Color color;

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
            TheClient.Rendering.SetColor(TheClient.Rendering.AdaptColor(ClientUtilities.Convert(GetPosition()), color));
            TheClient.Rendering.SetMinimumLight(0.0f);
            // TODO: Prevent model flipping (Possibly related to animation?)
            Matrix4 mat = PreRot * Matrix4.CreateRotationZ((float)(Direction.Yaw * Utilities.PI180)) * Matrix4.CreateTranslation(ClientUtilities.Convert(GetPosition()));
            GL.UniformMatrix4(2, false, ref mat);
            model.Draw(aHTime, hAnim, aTTime, tAnim, aLTime, lAnim);
            TheClient.Rendering.SetColor(Color4.White);
            // TODO: Render held item!
            if (IsTyping)
            {
                TheClient.Textures.GetTexture("ui/game/typing").Bind(); // TODO: store!
                TheClient.Rendering.RenderBillboard(GetPosition() + new Location(0, 0, 4), new Location(2), TheClient.CameraPos);
            }
        }
    }
}
