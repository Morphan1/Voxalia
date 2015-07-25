using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class BaseFJoint: InternalBaseJoint
    {
        public abstract void Solve();

        public override void Enable()
        {
            Enabled = true;
            One.TheWorld.SendToAll(new JointStatusPacketOut(this));
        }

        public override void Disable()
        {
            Enabled = false;
            One.TheWorld.SendToAll(new JointStatusPacketOut(this));
        }
    }
}
