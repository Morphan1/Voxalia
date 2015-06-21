using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;
using BEPUphysics.Constraints.TwoEntity;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.JointSystem
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

        public abstract TwoEntityConstraint GetBaseJoint();

        public TwoEntityConstraint CurrentJoint = null;

        public override void Enable()
        {
            if (CurrentJoint != null)
            {
                CurrentJoint.IsActive = true;
            }
            Enabled = true;
            One.TheServer.SendToAll(new JointStatusPacketOut(this));
        }

        public override void Disable()
        {
            if (CurrentJoint != null)
            {
                CurrentJoint.IsActive = false;
            }
            Enabled = false;
            One.TheServer.SendToAll(new JointStatusPacketOut(this));
        }
    }
}
