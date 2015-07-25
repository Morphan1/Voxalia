using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    public class NetStringPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            TheClient.Network.Strings.Strings.Add(FileHandler.encoding.GetString(data));
            return true;
        }
    }
}
