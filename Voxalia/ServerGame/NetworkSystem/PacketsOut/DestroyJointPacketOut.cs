using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.JointSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class DestroyJointPacketOut : AbstractPacketOut
    {
        public DestroyJointPacketOut(InternalBaseJoint joint)
        {
            ID = 14;
            Data = Utilities.LongToBytes(joint.JID);
        }
    }
}
