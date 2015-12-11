using BEPUphysics.Constraints;
using BEPUphysics.Constraints.TwoEntity.Motors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ClientGame.JointSystem
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
            return new RevoluteMotor(Ent1.Body, Ent2.Body, Direction.ToBVector());
        }

        public Location Direction;
    }
}
