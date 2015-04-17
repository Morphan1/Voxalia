using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class PhysicsEntityUpdatePacketOut: AbstractPacketOut
    {
        public PhysicsEntityUpdatePacketOut(PhysicsEntity e)
        {
            ID = 3;
            Data = new byte[12 + 12 + 12 + 12 + 1 + 8];
            e.GetPosition().ToBytes().CopyTo(Data, 0);
            e.GetVelocity().ToBytes().CopyTo(Data, 12);
            e.GetAngles().ToBytes().CopyTo(Data, 12 + 12);
            e.GetAngularVelocity().ToBytes().CopyTo(Data, 12 + 12 + 12);
            Data[12 + 12 + 12 + 12] = (byte)(e.Body.IsActive ? 1 : 0);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 12 + 12 + 12 + 12 + 1);
        }
    }
}
