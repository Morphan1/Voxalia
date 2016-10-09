//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.SolverGroups;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints;

namespace Voxalia.ServerGame.JointSystem
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
