using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.EntitySystem
{
    class SlimeEntity: CharacterEntity // TODO: More Generic-er
    {
        public Model model;
        
        public SlimeEntity(Region tregion)
            : base (tregion)
        {
            CBHHeight = 0.3f * 0.5f;
            CBStepHeight = 0.1f;
            CBDownStepHeight = 0.1f;
            CBRadius = 0.3f;
            SetMass(10);
        }

        public override void SpawnBody()
        {
            base.SpawnBody();
            model = TheClient.Models.LoadModel("mobs/slimes/slime");
            model.LoadSkin(TheClient.Textures);
        }

        public override void Render()
        {
            TheClient.Rendering.SetMinimumLight(0.0f);
            Matrix4 mat = GetTransformationMatrix();
            GL.UniformMatrix4(2, false, ref mat);
            model.Draw();
        }
    }
}
