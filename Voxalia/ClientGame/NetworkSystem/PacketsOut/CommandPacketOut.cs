using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    class CommandPacketOut: AbstractPacketOut
    {
        public CommandPacketOut(string cmd)
        {
            ID = ClientToServerPacket.COMMAND;
            Data = FileHandler.encoding.GetBytes(cmd);
        }
    }
}
