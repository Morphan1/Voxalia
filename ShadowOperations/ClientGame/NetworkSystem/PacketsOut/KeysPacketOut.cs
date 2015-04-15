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
            byte[] data1 = Utilities.UshortToBytes((ushort)data);
            byte[] data2 = Utilities.FloatToBytes((float)direction.X);
            byte[] data3 = Utilities.FloatToBytes((float)direction.Y);
            Data = new byte[data1.Length + data2.Length + data3.Length];
            data1.CopyTo(Data, 0);
            data2.CopyTo(Data, data1.Length);
            data3.CopyTo(Data, data1.Length + data2.Length);
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
