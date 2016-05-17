using Voxalia.ServerGame.JointSystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class DestroyJointPacketOut : AbstractPacketOut
    {
        public DestroyJointPacketOut(InternalBaseJoint joint)
        {
            ID = ServerToClientPacket.DESTROY_JOINT;
            Data = Utilities.LongToBytes(joint.JID);
        }
    }
}
