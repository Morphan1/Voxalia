using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class NetStringPacketOut: AbstractPacketOut
    {
        public NetStringPacketOut(string str)
        {
            ID = ServerToClientPacket.NET_STRING;
            Data = FileHandler.encoding.GetBytes(str);
        }
    }
}
