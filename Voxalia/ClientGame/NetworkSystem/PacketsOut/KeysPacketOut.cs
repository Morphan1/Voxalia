//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;

namespace Voxalia.ClientGame.NetworkSystem.PacketsOut
{
    public class KeysPacketOut: AbstractPacketOut
    {
        public KeysPacketOut(long tID, KeysPacketData data, Location direction, float xmove, float ymove, Location pos, Location vel, float sow)
        {
            ID = ClientToServerPacket.KEYS;
            Data = new byte[8 + 2 + 4 + 4 + 4 + 4 + 24 + 24 + 4];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            Utilities.UshortToBytes((ushort)data).CopyTo(Data, 8);
            Utilities.FloatToBytes((float)direction.Yaw).CopyTo(Data, 8 + 2);
            Utilities.FloatToBytes((float)direction.Pitch).CopyTo(Data, 8 + 2 + 4);
            Utilities.FloatToBytes(xmove).CopyTo(Data, 8 + 2 + 4 + 4);
            Utilities.FloatToBytes(ymove).CopyTo(Data, 8 + 2 + 4 + 4 + 4);
            int s = 8 + 2 + 4 + 4 + 4 + 4;
            pos.ToDoubleBytes().CopyTo(Data, s);
            vel.ToDoubleBytes().CopyTo(Data, s + 24);
            Utilities.FloatToBytes(sow).CopyTo(Data, s + 24 + 24);
        }
    }
}
