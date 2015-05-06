using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsIn
{
    class DestroyJointPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 8)
            {
                return false;
            }
            long JID = Utilities.BytesToLong(data);
            for (int i = 0; i < TheClient.Joints.Count; i++)
            {
                if (TheClient.Joints[i].JID == JID)
                {
                    TheClient.DestroyJoint(TheClient.Joints[i]);
                    return true;
                }
            }
            return false;
        }
    }
}
