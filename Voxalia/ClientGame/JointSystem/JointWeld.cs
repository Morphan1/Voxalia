using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SolverGroups;

namespace Voxalia.ClientGame.JointSystem
{
    class JointWeld: BaseJoint
    {
        public JointWeld(PhysicsEntity e1, PhysicsEntity e2)
        {
            Ent1 = e1;
            Ent2 = e2;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new WeldJoint(Ent1.Body, Ent2.Body);
        }
    }
}
