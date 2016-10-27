//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using OpenTK;
using Voxalia.ClientGame.ClientMainSystem;
using Voxalia.ClientGame.OtherSystems;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    class LightOrtho: Light
    {
        public override OpenTK.Matrix4 GetMatrix()
        {
            Vector3d c = ClientUtilities.ConvertD(Client.Central.MainWorldView.RenderRelative);
            Vector3d e = eye - c;
            Vector3d d = target - c;
            return Matrix4.LookAt(new Vector3((float)e.X, (float)e.Y, (float)e.Z), new Vector3((float)d.X, (float)d.Y, (float)d.Z), up) * Matrix4.CreateOrthographic(FOV, FOV, 1f, maxrange);
        }
    }
}
