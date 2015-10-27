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
            for (int i = 0; i < TheClient.TheRegion.Joints.Count; i++)
            {
                if (TheClient.TheRegion.Joints[i].JID == JID)
                {
                    TheClient.TheRegion.DestroyJoint(TheClient.TheRegion.Joints[i]);
                    return true;
                }
            }
            return false;
        }
    }
}
