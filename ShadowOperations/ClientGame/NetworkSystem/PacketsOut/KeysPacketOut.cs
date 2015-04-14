using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;

namespace ShadowOperations.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(KeysPacketData data)
        {
            ID = 1;
            Data = Utilities.UshortToBytes((ushort)data);
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
