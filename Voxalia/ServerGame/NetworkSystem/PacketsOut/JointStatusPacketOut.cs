using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.ServerGame.JointSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class JointStatusPacketOut: AbstractPacketOut
    {
        public JointStatusPacketOut(InternalBaseJoint joint)
        {
            ID = 20;
            Data = new byte[8 + 1];
            Utilities.LongToBytes(joint.JID).CopyTo(Data, 0);
            Data[8] = (byte)(joint.Enabled ? 1: 0);
        }
    }
}
