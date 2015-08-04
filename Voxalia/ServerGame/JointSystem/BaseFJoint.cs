using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class BaseFJoint: InternalBaseJoint
    {
        public abstract void Solve();

        public override void Enable()
        {
            Enabled = true;
            One.TheRegion.SendToAll(new JointStatusPacketOut(this));
        }

        public override void Disable()
        {
            Enabled = false;
            One.TheRegion.SendToAll(new JointStatusPacketOut(this));
        }
    }
}
