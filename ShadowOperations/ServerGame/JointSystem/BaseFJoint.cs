using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.EntitySystem;
using ShadowOperations.ServerGame.NetworkSystem.PacketsOut;

namespace ShadowOperations.ServerGame.JointSystem
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
