using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.Constraints;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointBallSocket : BaseJoint
    {
        public JointBallSocket(PhysicsEntity e1, PhysicsEntity e2, Location pos)
        {
            One = e1;
            Two = e2;
            Position = pos;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new BallSocketJoint(Ent1.Body, Ent2.Body, Position.ToBVector());
        }

        public override bool ApplyVar(Region tregion, string var, string value)
        {
            switch (var)
            {
                case "position":
                    Position = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(tregion, var, value);
            }
        }

        public Location Position;
    }
}
