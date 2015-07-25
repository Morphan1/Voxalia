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
            Location pos = Location.FromBytes(data, 0);
            TheClient.Player.SetPosition(pos);
            return true;
        }
    }
}
