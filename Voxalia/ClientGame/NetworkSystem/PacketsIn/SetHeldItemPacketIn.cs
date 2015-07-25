using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class SetHeldItemPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 4)
            {
                return false;
            }
            int dat = Utilities.BytesToInt(data);
            TheClient.QuickBarPos = dat;
            return true;
        }
    }
}
