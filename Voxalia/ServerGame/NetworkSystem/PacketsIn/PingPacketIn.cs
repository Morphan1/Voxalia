using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.NetworkSystem.PacketsOut;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class PingPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1)
            {
                return false;
            }
            if (data[0] != Player.LastPingByte)
            {
                return false;
            }
            Player.LastPingByte = (byte)Utilities.UtilRandom.Next(1, 255);
            Player.Network.SendPacket(new PingPacketOut(Player.LastPingByte));
            return true;
        }
    }
}
