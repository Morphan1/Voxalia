using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ClientGame.EntitySystem;
using BEPUutilities;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.JointSystem
{
    class JointForceWeld: BaseFJoint
    {
        public JointForceWeld(Entity one, Entity two)
        {
            One = one;
            Two = two;
            Matrix worldTrans = Matrix.CreateFromQuaternion(One.GetOrientation()) * Matrix.CreateTranslation(One.GetPosition().ToBVector());
            Matrix.Invert(ref worldTrans, out worldTrans);
            Relative = (Matrix.CreateFromQuaternion(two.GetOrientation())
                * Matrix.CreateTranslation(two.GetPosition().ToBVector()))
                * worldTrans;
        }

        public Matrix Relative;

        public override void Solve()
        {
            Matrix worldTrans = Matrix.CreateFromQuaternion(One.GetOrientation()) * Matrix.CreateTranslation(One.GetPosition().ToBVector());
            Matrix tmat = Relative * worldTrans;
            Location pos = Location.FromBVector(tmat.Translation);
            Quaternion quat = Quaternion.CreateFromRotationMatrix(tmat);
            Two.SetPosition(pos);
            Two.SetOrientation(quat);
        }
    }
}
