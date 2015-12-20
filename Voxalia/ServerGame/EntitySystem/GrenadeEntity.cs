using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.EntitySystem
{
    public abstract class GrenadeEntity : PhysicsEntity
    {
        public GrenadeEntity(Region tregion) :
            base(tregion)
        {
            ConvexEntityShape = new CylinderShape(0.2f, 0.05f);
            Shape = ConvexEntityShape;
            Bounciness = 0.95f;
            SetMass(1);
        }
    }
}
