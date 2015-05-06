using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ClientGame.EntitySystem;
using BEPUphysics.Constraints.TwoEntity;

namespace ShadowOperations.ClientGame.JointSystem
{
    public abstract class BaseJoint
    {
        public PhysicsEntity Ent1;
        public PhysicsEntity Ent2;

        public long JID;

        public abstract TwoEntityConstraint GetBaseJoint();

        public TwoEntityConstraint CurrentJoint;
    }
}
