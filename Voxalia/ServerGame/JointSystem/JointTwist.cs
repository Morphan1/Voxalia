using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using Voxalia.ServerGame.WorldSystem;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointTwist : BaseJoint
    {
        public JointTwist(PhysicsEntity e1, PhysicsEntity e2, Location a1, Location a2)
        {
            Ent1 = e1;
            Ent2 = e2;
            AxisOne = a1;
            AxisTwo = a2;
        }

        public override TwoEntityConstraint GetBaseJoint()
        {
            return new TwistJoint(Ent1.Body, Ent2.Body, AxisOne.ToBVector(), AxisTwo.ToBVector());
        }

        public override bool ApplyVar(Region tworld, string var, string value)
        {
            switch (var)
            {
                case "axis1":
                    AxisOne = Location.FromString(value);
                    return true;
                case "axis2":
                    AxisTwo = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(tworld, var, value);
            }
        }

        public Location AxisOne;
        public Location AxisTwo;
    }
}
