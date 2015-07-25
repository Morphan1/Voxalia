using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class MessagePacketOut: AbstractPacketOut
    {
        public MessagePacketOut(string msg)
        {
            ID = 5;
            Data = FileHandler.encoding.GetBytes(msg);
        }
    }
}
