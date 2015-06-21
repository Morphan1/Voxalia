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
            One = e1;
            Two = e2;
            Position = pos;
        }

        public override TwoEntityConstraint GetBaseJoint()
        {
            return new BallSocketJoint(Ent1.Body, Ent2.Body, Position.ToBVector());
        }

        public override bool ApplyVar(ServerMainSystem.Server tserver, string var, string value)
        {
            switch (var)
            {
                case "position":
                    Position = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(tserver, var, value);
            }
        }

        public Location Position;
    }
}
