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
    public class PhysicsEntityUpdatePacketOut: AbstractPacketOut
    {
        public PhysicsEntityUpdatePacketOut(PhysicsEntity e)
        {
            UsageType = NetUsageType.ENTITIES;
            ID = ServerToClientPacket.PHYSICS_ENTITY_UPDATE;
            Data = new byte[24 + 24 + 16 + 24 + 1 + 8];
            e.GetPosition().ToDoubleBytes().CopyTo(Data, 0);
            e.GetVelocity().ToDoubleBytes().CopyTo(Data, 24);
            Utilities.QuaternionToBytes(e.GetOrientation()).CopyTo(Data, 24 + 24);
            e.GetAngularVelocity().ToDoubleBytes().CopyTo(Data, 24 + 24 + 16);
            Data[24 + 24 + 16 + 24] = (byte)((e.Body != null && e.Body.ActivityInformation.IsActive) ? 1 : 0);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 24 + 24 + 16 + 24 + 1);
        }
    }
}
