using Voxalia.ServerGame.EntitySystem;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.TwoEntity;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class BaseJoint : InternalBaseJoint
    {
        public PhysicsEntity Ent1
        {
            get
            {
                return (PhysicsEntity)One;
            }
            set
            {
                One = value;
            }
        }

        public PhysicsEntity Ent2
        {
            get
            {
                return (PhysicsEntity)Two;
            }
            set
            {
                Two = value;
            }
        }

        public abstract SolverUpdateable GetBaseJoint();

        public SolverUpdateable CurrentJoint = null;

        public override void Enable()
        {
            if (CurrentJoint != null)
            {
                CurrentJoint.IsActive = true;
            }
            Enabled = true;
            One.TheRegion.SendToAll(new JointStatusPacketOut(this));
        }

        public override void Disable()
        {
            if (CurrentJoint != null)
            {
                CurrentJoint.IsActive = false;
            }
            Enabled = false;
            One.TheRegion.SendToAll(new JointStatusPacketOut(this));
        }
    }
}
