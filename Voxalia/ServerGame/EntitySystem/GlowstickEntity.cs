using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.EntitySystem
{
    public class GlowstickEntity: GrenadeEntity
    {
        public int Color;

        public GlowstickEntity(int col, Region tregion) :
            base(tregion)
        {
            Color = col;
        }
    }
}
