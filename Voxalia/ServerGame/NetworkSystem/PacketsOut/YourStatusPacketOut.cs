using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourStatusPacketOut: AbstractPacketOut
    {
        public YourStatusPacketOut(float health, float max_health, YourStatusFlags flags)
        {
            ID = 11;
            Data = new byte[4 + 4 + 1];
            Utilities.FloatToBytes(health).CopyTo(Data, 0);
            Utilities.FloatToBytes(max_health).CopyTo(Data, 4);
            Data[4 + 4] = (byte)flags;
        }
    }
}
