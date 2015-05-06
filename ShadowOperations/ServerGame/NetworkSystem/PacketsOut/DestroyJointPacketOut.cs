using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.ServerGame.JointSystem;
using ShadowOperations.Shared;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class DestroyJointPacketOut : AbstractPacketOut
    {
        public DestroyJointPacketOut(BaseJoint joint)
        {
            ID = 14;
            Data = Utilities.LongToBytes(joint.JID);
        }
    }
}
