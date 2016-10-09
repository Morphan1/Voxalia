//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System.Collections.Generic;
using Voxalia.Shared;

namespace Voxalia.ClientGame.GraphicsSystems.LightingSystem
{
    public abstract class LightObject
    {
        public List<Light> InternalLights = new List<Light>();

        public Location EyePos;

        public float MaxDistance;

        public abstract void Reposition(Location pos);
    }
}
