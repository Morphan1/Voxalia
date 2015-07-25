using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class InternalBaseJoint
    {
        public Entity One;
        public Entity Two;

        public long JID;

        public abstract void Disable();

        public abstract void Enable();

        public bool Enabled = false;

        public virtual bool ApplyVar(World tworld, string var, string value)
        {
            switch (var)
            {
                case "one":
                    return tworld.JointTargets.TryGetValue(value, out One);
                case "two":
                    return tworld.JointTargets.TryGetValue(value, out Two);
                default:
                    return false;
            }
        }
    }
}
