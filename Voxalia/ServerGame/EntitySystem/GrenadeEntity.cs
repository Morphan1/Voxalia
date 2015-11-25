using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.CollisionShapes.ConvexShapes;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.EntitySystem
{
    public class GrenadeEntity : PhysicsEntity
    {
        public GrenadeEntity(Region tregion) :
            base(tregion, true)
        {
            ConvexEntityShape = new CylinderShape(0.2f, 0.05f);
            Shape = ConvexEntityShape;
            Bounciness = 0.95f;
            SetMass(1);
        }

        bool pActive = false;

        public double deltat = 0;

        public override void Tick()
        {
            // TODO: Generic physent method for all this
            if (Body == null)
            {
                return;
            }
            if (Body.ActivityInformation.IsActive || (pActive && !Body.ActivityInformation.IsActive))
            {
                pActive = Body.ActivityInformation.IsActive;
                TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
            }
            if (!pActive && GetMass() > 0)
            {
                deltat += TheRegion.Delta;
                if (deltat > 2.0)
                {
                    TheRegion.SendToAll(new PhysicsEntityUpdatePacketOut(this));
                }
            }
            base.Tick();
        }
    }
}
