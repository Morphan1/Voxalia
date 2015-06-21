using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PhysicsEntityUpdatePacketOut: AbstractPacketOut
    {
        public PhysicsEntityUpdatePacketOut(PhysicsEntity e)
        {
            ID = 3;
            Data = new byte[12 + 12 + 16 + 12 + 1 + 8];
            e.GetPosition().ToBytes().CopyTo(Data, 0);
            e.GetVelocity().ToBytes().CopyTo(Data, 12);
            Utilities.QuaternionToBytes(e.GetOrientation()).CopyTo(Data, 12 + 12);
            e.GetAngularVelocity().ToBytes().CopyTo(Data, 12 + 12 + 16);
            Data[12 + 12 + 16 + 12] = (byte)(e.Body.ActivityInformation.IsActive ? 1 : 0);
            Utilities.LongToBytes(e.EID).CopyTo(Data, 12 + 12 + 16 + 12 + 1);
        }
    }
}
