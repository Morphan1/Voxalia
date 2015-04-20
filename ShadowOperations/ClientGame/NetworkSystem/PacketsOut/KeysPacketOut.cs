using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(KeysPacketData data, Location direction)
        {
            ID = 1;
            Data = new byte[2 + 4 + 4];
            Utilities.UshortToBytes((ushort)data).CopyTo(Data, 0);
            Utilities.FloatToBytes((float)direction.X).CopyTo(Data, 2);
            Utilities.FloatToBytes((float)direction.Y).CopyTo(Data, 2 + 4);
        }
    }

    public enum KeysPacketData: ushort
    {
        FORWARD = 1,
        BACKWARD = 2,
        LEFTWARD = 4,
        RIGHTWARD = 8,
        UPWARD = 16,
        DOWNWARD = 32
    }
}
