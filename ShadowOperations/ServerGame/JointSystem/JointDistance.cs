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
        public JointDistance(PhysicsEntity e1, PhysicsEntity e2, float min, float max, Location e1pos, Location e2pos)
        {
            Ent1 = e1;
            Ent2 = e2;
            Min = min;
            Max = max;
            Ent1Pos = e1pos - e1.GetPosition();
            Ent2Pos = e2pos - e2.GetPosition();
        }

        public float Min;
        public float Max;
        public Location Ent1Pos;
        public Location Ent2Pos;

        public override TwoEntityConstraint GetBaseJoint()
        {
            DistanceLimit dl = new DistanceLimit(Ent1.Body, Ent2.Body, (Ent1Pos + Ent1.GetPosition()).ToBVector(), (Ent2Pos + Ent2.GetPosition()).ToBVector(), Min, Max);
            //dl.MaxCorrectiveVelocity = 10000f;
            //dl.SpringSettings.Stiffness = 10000f;
            return dl;
        }
    }
}
