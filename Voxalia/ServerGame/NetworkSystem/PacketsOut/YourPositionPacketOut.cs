//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using BEPUphysics.Character;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class YourPositionPacketOut: AbstractPacketOut
    {
        public YourPositionPacketOut(double delta, long tID, Location pos, Location vel, Location avel, Stance stance, bool pup)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.YOUR_POSITION;
            Data = new byte[8 + 24 + 24 + 1 + 8];
            Utilities.LongToBytes(tID).CopyTo(Data, 0);
            pos.ToDoubleBytes().CopyTo(Data, 8);
            vel.ToDoubleBytes().CopyTo(Data, 8 + 24);
            Data[8 + 24 + 24] = (byte)((stance == Stance.Standing ? 0 : 1) | (pup ? 2: 0));
            Utilities.DoubleToBytes(delta).CopyTo(Data, 8 + 24 + 24 + 1);
        }
    }
}
