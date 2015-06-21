using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.EntitySystem;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class BaseFJoint: InternalBaseJoint
    {
        public abstract void Solve();

        public override void Enable()
        {
            Enabled = true;
            One.TheServer.SendToAll(new JointStatusPacketOut(this));
        }

        public override void Disable()
        {
            Enabled = false;
            One.TheServer.SendToAll(new JointStatusPacketOut(this));
        }
    }
}
