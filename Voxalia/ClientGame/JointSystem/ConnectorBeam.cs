using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Voxalia.Shared;

namespace Voxalia.ClientGame.JointSystem
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
