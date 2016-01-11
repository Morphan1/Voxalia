using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUutilities;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.TwoEntity.Motors;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointVehicleMotor : BaseJoint
    {
        public JointVehicleMotor(PhysicsEntity e1, PhysicsEntity e2, Location dir, bool isSteering)
        {
            Ent1 = e1;
            Ent2 = e2;
            Direction = dir;
            IsSteering = isSteering;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            Motor = new RevoluteMotor(Ent1.Body, Ent2.Body, Direction.ToBVector());
            if (IsSteering)
            {
                Motor.Settings.Mode = MotorMode.Servomechanism;
                Motor.Basis.SetWorldAxes(Vector3.UnitZ, Vector3.UnitX);
                Motor.TestAxis = Vector3.UnitX;
                Motor.Settings.Servo.BaseCorrectiveSpeed = 5;
            }
            else
            {
                Motor.Settings.Mode = MotorMode.VelocityMotor;
                Motor.Settings.VelocityMotor.Softness = 0.3f;
                Motor.Settings.MaximumForce = 10000;
            }
            return Motor;
        }

        public Location Direction;

        public bool IsSteering = false;

        public RevoluteMotor Motor;
    }
}
