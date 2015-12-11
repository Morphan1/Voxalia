using Voxalia.ClientGame.EntitySystem;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Constraints;
using Voxalia.Shared;

namespace Voxalia.ClientGame.JointSystem
{
    class JointPullPush : BaseJoint
    {
        public JointPullPush(PhysicsEntity e1, PhysicsEntity e2, float stren, Location axis)
        {
            Ent1 = e1;
            Ent2 = e2;
            Strength = stren;
            Axis = axis;
        }

        public float Strength;

        public Location Axis;

        public override SolverUpdateable GetBaseJoint()
        {
            LinearAxisMotor lam = new LinearAxisMotor(Ent1.Body, Ent2.Body, Ent1.GetPosition().ToBVector(), Ent2.GetPosition().ToBVector(), Axis.ToBVector());
            lam.Settings.Mode = MotorMode.VelocityMotor;
            lam.Settings.MaximumForce = 100 * 5;
            lam.Settings.VelocityMotor.Softness = 0.01f;
            lam.Settings.VelocityMotor.GoalVelocity = Strength;
            return lam;
        }
    }
}
