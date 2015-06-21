using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
