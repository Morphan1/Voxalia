using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
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
            for (int i = 0; i < TheClient.TheWorld.Joints.Count; i++)
            {
                if (TheClient.TheWorld.Joints[i].JID == JID)
                {
                    TheClient.TheWorld.DestroyJoint(TheClient.TheWorld.Joints[i]);
                    return true;
                }
            }
            return false;
        }
    }
}
