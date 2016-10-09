//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ServerGame.EntitySystem;
using BEPUutilities;
using Voxalia.Shared;

namespace Voxalia.ServerGame.JointSystem
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
            Location pos = new Location(tmat.Translation);
            Quaternion quat = Quaternion.CreateFromRotationMatrix(tmat);
            Two.SetPosition(pos);
            Two.SetOrientation(quat);
        }
    }
}
