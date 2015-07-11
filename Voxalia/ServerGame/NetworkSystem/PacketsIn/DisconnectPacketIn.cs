using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class DisconnectPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 1 || data[0] != 0)
            {
                return false;
            }
            Player.Kick("Willful disconnect.");
            return true;
        }
    }
}
