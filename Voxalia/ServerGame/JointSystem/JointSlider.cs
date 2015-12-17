using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using Voxalia.ServerGame.WorldSystem;
using BEPUphysics.Constraints;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointSlider : BaseJoint
    {
        public JointSlider(PhysicsEntity e1, PhysicsEntity e2, Location dir)
        {
            Ent1 = e1;
            Ent2 = e2;
            Direction = dir;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new PointOnLineJoint(Ent1.Body, Ent2.Body, Ent2.GetPosition().ToBVector(), Direction.Normalize().ToBVector(), Ent2.GetPosition().ToBVector());
        }

        public override bool ApplyVar(Region tregion, string var, string value)
        {
            switch (var)
            {
                case "direction":
                    Direction = Location.FromString(value);
                    return true;
                default:
                    return base.ApplyVar(tregion, var, value);
            }
        }

        public Location Direction;
    }
}
