using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.ServerMainSystem;

namespace ShadowOperations.ServerGame.JointSystem
{
    public abstract class InternalBaseJoint
    {
        public Entity One;
        public Entity Two;

        public long JID;

        public abstract void Disable();

        public abstract void Enable();

        public bool Enabled = false;

        public virtual bool ApplyVar(Server tserver, string var, string value)
        {
            switch (var)
            {
                case "one":
                    One = tserver.JointTargets[value];
                    return true;
                case "two":
                    Two = tserver.JointTargets[value];
                    return true;
                default:
                    return false;
            }
        }
    }
}
