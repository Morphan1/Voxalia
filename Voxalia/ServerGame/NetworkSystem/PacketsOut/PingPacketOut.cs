using System;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PingPacketOut: AbstractPacketOut
    {
        public PingPacketOut(byte bit)
        {
            UsageType = NetUsageType.PINGS;
            ID = ServerToClientPacket.PING;
            Data = new byte[] { bit };
        }
    }
}
