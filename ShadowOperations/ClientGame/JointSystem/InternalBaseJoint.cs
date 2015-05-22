using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.EntitySystem;

namespace ShadowOperations.ClientGame.JointSystem
{
    public abstract class InternalBaseJoint
    {
        public Entity One;
        public Entity Two;

        public long JID;
    }
}
