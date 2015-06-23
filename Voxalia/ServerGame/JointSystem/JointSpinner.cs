using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointSpinner: BaseJoint
    {
        public JointSpinner(PhysicsEntity e1, PhysicsEntity e2, Location dir)
        {
            One = e1;
            Two = e2;
            Direction = dir;
        }

        public override TwoEntityConstraint GetBaseJoint()
        {
            return new RevoluteAngularJoint(Ent1.Body, Ent2.Body, Direction.ToBVector());
        }

        public override bool ApplyVar(World tworld, string var, string value)
        {
            switch (var)
            {
                case "direction":
                    Direction = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(tworld, var, value);
            }
        }

        public Location Direction;
    }
}
