using System;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class PingPacketOut: AbstractPacketOut
    {
        public PingPacketOut(byte bit)
        {
            ID = ClientToServerPacket.PING;
            Data = new byte[] { bit };
        }
    }
}
