using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.JointSystem
{
    public abstract class BaseFJoint: InternalBaseJoint
    {
        public abstract void Solve();

        public override void Enable()
        {
            Enabled = true;
            //TODO: Transmit!
        }

        public override void Disable()
        {
            Enabled = false;
            //TODO: Transmit!
        }
    }
}
