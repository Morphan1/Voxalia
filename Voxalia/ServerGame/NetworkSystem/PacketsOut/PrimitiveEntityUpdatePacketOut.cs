using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Voxalia.Shared;
using Voxalia.ServerGame.EntitySystem;

namespace Voxalia.ServerGame.NetworkSystem.PacketsOut
{
    public class PrimitiveEntityUpdatePacketOut: AbstractPacketOut
    {
        public PrimitiveEntityUpdatePacketOut(PrimitiveEntity pe)
        {
            ID = 16;
            Data = new byte[12 + 12 + 16 + 12 + 8];
            pe.GetPosition().ToBytes().CopyTo(Data, 0);
            pe.GetVelocity().ToBytes().CopyTo(Data, 12);
            Utilities.QuaternionToBytes(pe.Angles).CopyTo(Data, 12 + 12);
            pe.Gravity.ToBytes().CopyTo(Data, 12 + 12 + 16);
            Utilities.LongToBytes(pe.EID).CopyTo(Data, 12 + 12 + 16 + 12);
        }
    }
}
