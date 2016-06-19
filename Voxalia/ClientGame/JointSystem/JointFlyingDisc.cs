﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BEPUphysics.Constraints;
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared.Collision;

namespace Voxalia.ClientGame.JointSystem
{
    public class JointFlyingDisc : BaseJoint
    {
        public JointFlyingDisc(Entity e)
        {
            One = e;
            Two = e;
        }

        public override SolverUpdateable GetBaseJoint()
        {
            return new FlyingDiscConstraint(Ent1.Body);
        }
    }
}