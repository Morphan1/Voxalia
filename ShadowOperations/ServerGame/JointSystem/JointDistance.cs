using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.Shared;
using BEPUphysics.Constraints.TwoEntity;
using BEPUphysics.Constraints.TwoEntity.JointLimits;

namespace ShadowOperations.ServerGame.JointSystem
{
    public class JointDistance : BaseJoint
    {
        public JointDistance(PhysicsEntity e1, PhysicsEntity e2, float min, float max)
        {
            Ent1 = e1;
            Ent2 = e2;
            Min = min;
            Max = max;
        }

        public float Min;
        public float Max;

        public override TwoEntityConstraint GetBaseJoint()
        {
            DistanceLimit dl = new DistanceLimit(Ent1.Body, Ent2.Body, Ent1.GetPosition().ToBVector(), Ent2.GetPosition().ToBVector(), Min, Max);
            dl.Bounciness = 0.001f;
            dl.MaxCorrectiveVelocity = 0.5f;
            return dl;
        }
    }
}
