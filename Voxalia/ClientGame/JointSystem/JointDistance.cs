//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ClientGame.EntitySystem;
using Voxalia.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.JointLimits;
using BEPUphysics.Constraints;

namespace Voxalia.ClientGame.JointSystem
{
    public class JointDistance : BaseJoint
    {
        public JointDistance(PhysicsEntity e1, PhysicsEntity e2, float min, float max, Location e1pos, Location e2pos)
        {
            Ent1 = e1;
            Ent2 = e2;
            Min = min;
            Max = max;
            Ent1Pos = e1pos;
            Ent2Pos = e2pos;
        }

        public float Min;
        public float Max;
        public Location Ent1Pos;
        public Location Ent2Pos;

        public override SolverUpdateable GetBaseJoint()
        {
            DistanceLimit dl = new DistanceLimit(Ent1.Body, Ent2.Body, (Ent1Pos + Ent1.GetPosition()).ToBVector(), (Ent2Pos + Ent2.GetPosition()).ToBVector(), Min, Max);
            return dl;
        }
    }
}
