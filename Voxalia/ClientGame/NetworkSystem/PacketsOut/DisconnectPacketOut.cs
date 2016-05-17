using System;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class DisconnectPacketOut: AbstractPacketOut
    {
        public DisconnectPacketOut()
        {
            ID = ClientToServerPacket.DISCONNECT;
            Data = new byte[] { 0 };
        }
    }
}
