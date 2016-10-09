//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.ClientGame.WorldSystem;
using Voxalia.ClientGame.GraphicsSystems;
using Voxalia.Shared;
using Voxalia.Shared.Collision;
using Voxalia.ClientGame.OtherSystems;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace Voxalia.ClientGame.EntitySystem
{
    class PrimitiveModelEntity : PrimitiveEntity
    {
        public Model model;

        public Location scale = Location.One;

        public PrimitiveModelEntity(string modelname, Region tregion)
            : base(tregion, false)
        {
            model = tregion.TheClient.Models.GetModel(modelname);
            Gravity = Location.Zero;
            Velocity = Location.Zero;
        }

        public BEPUutilities.Vector3 ModelMin;

        public BEPUutilities.Vector3 ModelMax;

        public override void Spawn()
        {
            List<BEPUutilities.Vector3> vecs = TheClient.Models.Handler.GetCollisionVertices(model.Original);
            Location zero = new Location(vecs[0]);
            AABB abox = new AABB() { Min = zero, Max = zero };
            for (int v = 1; v < vecs.Count; v++)
            {
                abox.Include(new Location(vecs[v]));
            }
            ModelMin = abox.Min.ToBVector();
            ModelMax = abox.Max.ToBVector();
        }

        public override void Destroy()
        {
        }

        public override void Render()
        {
            if (!Visible)
            {
                return;
            }
            TheClient.SetEnts();
            BEPUutilities.RigidTransform rt = new BEPUutilities.RigidTransform(GetPosition().ToBVector(), GetOrientation());
            BEPUutilities.Vector3 bmin;
            BEPUutilities.Vector3 bmax;
            BEPUutilities.RigidTransform.Transform(ref ModelMin, ref rt, out bmin);
            BEPUutilities.RigidTransform.Transform(ref ModelMax, ref rt, out bmax);
            if (TheClient.MainWorldView.CFrust != null && !TheClient.MainWorldView.CFrust.ContainsBox(bmin, bmax))
            {
                return;
            }
            Matrix4d orient = GetOrientationMatrix();
            Matrix4d mat = (Matrix4d.Scale(ClientUtilities.ConvertD(scale)) * orient * Matrix4d.CreateTranslation(ClientUtilities.ConvertD(GetPosition())));
            TheClient.MainWorldView.SetMatrix(2, mat);
            TheClient.Rendering.SetMinimumLight(0.0f);
            if (model.Meshes[0].vbo.Tex == null)
            {
                TheClient.Textures.White.Bind();
            }
            model.Draw(); // TODO: Animation?
        }

    }
}
