using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class TeleportPacketIn : AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 12)
            {
                return false;
            }
            TheClient.Player.SetPosition(Location.FromDoubleBytes(data, 0));
            return true;
        }
    }
}
