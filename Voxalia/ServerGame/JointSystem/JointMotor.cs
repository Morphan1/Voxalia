using BEPUphysics.Constraints;
using BEPUphysics.Constraints.TwoEntity.Motors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.JointSystem
{
    public class JointMotor : BaseJoint
    {
        public JointMotor(PhysicsEntity e1, PhysicsEntity e2, Location dir)
        {
            Ent1 = e1;
            Ent2 = e2;
            Direction = dir;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return Motor = new RevoluteMotor(Ent1.Body, Ent2.Body, Direction.ToBVector());
        }

        public Location Direction;
        public RevoluteMotor Motor;
    }
}
