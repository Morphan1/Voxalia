using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.EntitySystem;

namespace Voxalia.ClientGame.JointSystem
{
    public abstract class InternalBaseJoint
    {
        public Entity One;
        public Entity Two;

        public long JID;

        public abstract void Disable();

        public abstract void Enable();

        public bool Enabled = false;
    }
}
