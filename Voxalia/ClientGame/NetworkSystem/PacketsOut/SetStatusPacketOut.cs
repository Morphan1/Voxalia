using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class SetStatusPacketOut: AbstractPacketOut
    {
        public SetStatusPacketOut(ClientStatus opt, byte status)
        {
            ID = 5;
            Data = new byte[2];
            Data[0] = (byte)opt;
            Data[1] = status;
        }
    }
}
