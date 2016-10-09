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
using System.Threading.Tasks;
using Voxalia.Shared;

namespace Voxalia.ServerGame.JointSystem
{
    public class ConnectorBeam : BaseFJoint
    {
        public System.Drawing.Color color = System.Drawing.Color.Cyan;

        public override void Solve()
        {
            // Do nothing.
        }

        public BeamType type;
    }
}
