using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class MessagePacketOut: AbstractPacketOut
    {
        public MessagePacketOut(string msg)
        {
            ID = ServerToClientPacket.MESSAGE;
            Data = FileHandler.encoding.GetBytes(msg);
        }
    }
}
