using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsIn
{
    public class SetStatusPacketIn: AbstractPacketIn
    {
        public override bool ParseBytesAndExecute(byte[] data)
        {
            if (data.Length != 2)
            {
                return false;
            }
            switch ((ClientStatus)data[0])
            {
                case ClientStatus.TYPING:
                    Player.SetTypingStatus(data[1] != 0);
                    return true;
                default:
                    return false;
            }
        }
    }
}
