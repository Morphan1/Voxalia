using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;

namespace Voxalia.ServerGame.EntitySystem
{
    public class GlowstickEntity: PhysicsEntity
    {
        public int Color;

        public GlowstickEntity(int col, Region tregion):
            base(tregion, true)
        {
            ConvexEntityShape = new CylinderShape(0.2f, 0.05f);
            Shape = ConvexEntityShape;
            Color = col;
            Bounciness = 0.95f;
            SetMass(1);
        }
    }
}
