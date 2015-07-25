using Voxalia.ClientGame.UISystem;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsIn
{
    class MessagePacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            UIConsole.WriteLine(FileHandler.encoding.GetString(data));
            return true;
        }
    }
}
