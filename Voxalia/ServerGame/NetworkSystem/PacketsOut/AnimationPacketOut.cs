//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.ServerGame.EntitySystem;
using Voxalia.Shared;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    class AnimationPacketOut: AbstractPacketOut
    {
        public AnimationPacketOut(Entity e, string anim, byte mode)
        {
            UsageType = NetUsageType.PLAYERS;
            ID = ServerToClientPacket.ANIMATION;
            Data = new byte[8 + 4 + 1];
            Utilities.LongToBytes(e.EID).CopyTo(Data, 0);
            Utilities.IntToBytes(e.TheServer.Networking.Strings.IndexForString(anim)).CopyTo(Data, 8);
            Data[8 + 4] = mode;
        }
    }
}
