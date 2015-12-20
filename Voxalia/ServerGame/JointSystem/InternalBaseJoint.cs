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
    }
}
