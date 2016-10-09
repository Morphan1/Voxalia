//
// This file is part of the game Voxalia, created by FreneticXYZ.
// This code is Copyright (C) 2016 FreneticXYZ under the terms of the MIT license.
// See README.md or LICENSE.txt for contents of the MIT license.
// If these are not available, see https://opensource.org/licenses/MIT
//

using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PrimitiveEntityUpdatePacketOut: AbstractPacketOut
    {
        public PrimitiveEntityUpdatePacketOut(PrimitiveEntity pe)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.PRIMITIVE_ENTITY_UPDATE;
            Data = new byte[24 + 24 + 16 + 24 + 8];
            pe.GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            pe.GetVelocity().ToDoubleBytes().CopyTo(Data, 24);
            Utilities.QuaternionToBytes(pe.Angles).CopyTo(Data, 24 + 24);
            pe.Gravity.ToDoubleBytes().CopyTo(Data, 24 + 24 + 16);
            Utilities.LongToBytes(pe.EID).CopyTo(Data, 24 + 24 + 16 + 24);
        }
    }
}
