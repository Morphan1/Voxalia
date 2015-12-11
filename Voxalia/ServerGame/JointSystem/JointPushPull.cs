using Voxalia.ServerGame.EntitySystem;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Motors;
using BEPUphysics.Constraints;
using Voxalia.Shared;

namespace Voxalia.ServerGame.JointSystem
{
    class JointPullPush : BaseJoint
    {
        public JointPullPush(PhysicsEntity e1, PhysicsEntity e2, Location axis, bool mode)
        {
            Ent1 = e1;
            Ent2 = e2;
            Axis = axis;
            Mode = mode;
        }

        public Location Axis;

        public bool Mode;
        
        public override SolverUpdateable GetBaseJoint()
        {
            LinearAxisMotor lam = new LinearAxisMotor(Ent1.Body, Ent2.Body, Ent1.GetPosition().ToBVector(), Ent2.GetPosition().ToBVector(), Axis.ToBVector());
            lam.Settings.Mode = Mode ? MotorMode.Servomechanism : MotorMode.VelocityMotor;
            lam.Settings.Servo.Goal = 0;
            lam.Settings.Servo.SpringSettings.Stiffness = 300;
            lam.Settings.Servo.SpringSettings.Damping = 70;
            return lam;
        }
    }
}
