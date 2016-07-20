using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ClientGame.EntitySystem;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.SolverGroups;
using BEPUutilities;

namespace Voxalia.ClientGame.JointSystem
{
    public class JointWeld: BaseJoint
    {
        public JointWeld(PhysicsEntity e1, PhysicsEntity e2)
        {
            Ent1 = e1;
            Ent2 = e2;
        }

        public RigidTransform Relative;

        public override SolverUpdateable GetBaseJoint()
        {
            RigidTransform rt1 = new RigidTransform(Ent1.Body.Position, Ent1.Body.Orientation);
            RigidTransform rt2 = new RigidTransform(Ent2.Body.Position, Ent2.Body.Orientation);
            RigidTransform.MultiplyByInverse(ref rt1, ref rt2, out Relative);
            return new WeldJoint(Ent1.Body, Ent2.Body);
        }

        public override void Enable()
        {
            if (One is PlayerEntity)
            {
                ((PlayerEntity)One).Welded = this;
            }
            else if (Two is PlayerEntity)
            {
                ((PlayerEntity)Two).Welded = this;
            }
            base.Enable();
        }

        public override void Disable()
        {
            if (One is PlayerEntity)
            {
                ((PlayerEntity)One).Welded = null;
            }
            else if (Two is PlayerEntity)
            {
                ((PlayerEntity)Two).Welded = null;
            }
            base.Disable();
        }
    }
}
