using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    class CommandPacketOut: AbstractPacketOut
    {
        public CommandPacketOut(string cmd)
        {
            ID = 2;
            Data = FileHandler.encoding.GetBytes(cmd);
        }
    }
}
