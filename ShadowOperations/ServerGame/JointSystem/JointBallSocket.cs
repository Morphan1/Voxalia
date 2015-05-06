using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;

namespace ShadowOperations.ServerGame.JointSystem
{
    public class JointBallSocket : BaseJoint
    {
        public JointBallSocket(PhysicsEntity e1, PhysicsEntity e2, Location pos)
        {
            Ent1 = e1;
            Ent2 = e2;
            Position = pos;
        }

        public override TwoEntityConstraint GetBaseJoint()
        {
            return new BallSocketJoint(Ent1.Body, Ent2.Body, Position.ToBVector());
        }

        public Location Position;
    }
}
