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
using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints;
using BEPUphysics.Constraints.TwoEntity.JointLimits;

namespace Voxalia.ClientGame.JointSystem
{
    class JointLAxisLimit : BaseJoint
    {
        public JointLAxisLimit(PhysicsEntity e1, PhysicsEntity e2, float min, float max, Location cpos1, Location cpos2, Location axis)
        {
            Ent1 = e1;
            Ent2 = e2;
            Min = min;
            Max = max;
            CPos1 = cpos1;
            CPos2 = cpos2;
            Axis = axis;
        }

        public float Min;
        public float Max;
        public Location CPos1;
        public Location CPos2;
        public Location Axis;

        public override SolverUpdateable GetBaseJoint()
        {
            return new LinearAxisLimit(Ent1.Body, Ent2.Body, CPos1.ToBVector(), CPos2.ToBVector(), Axis.ToBVector(), Min, Max);
        }
    }
}
