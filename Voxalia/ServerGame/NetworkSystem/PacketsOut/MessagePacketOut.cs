using System;
using Voxalia.Shared;
using Voxalia.Shared.Files;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class MessagePacketOut: AbstractPacketOut
    {
        public MessagePacketOut(TextChannel chan, string msg)
        {
            UsageType = NetUsageType.GENERAL;
            ID = ServerToClientPacket.MESSAGE;
            byte[] text = FileHandler.encoding.GetBytes(msg);
            Data = new byte[1 + text.Length];
            Data[0] = (byte)chan;
            text.CopyTo(Data, 1);
        }
    }
}
