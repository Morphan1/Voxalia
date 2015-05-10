using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShadowOperations.Shared;
using ShadowOperations.ServerGame.EntitySystem;

namespace ShadowOperations.ServerGame.NetworkSystem.PacketsOut
{
    public class PrimitiveEntityUpdatePacketOut: AbstractPacketOut
    {
        public PrimitiveEntityUpdatePacketOut(PrimitiveEntity pe)
        {
            ID = 16;
            Data = new byte[12 + 12 + 12 + 12 + 8];
            pe.GetPosition().ToBytes().CopyTo(Data, 0);
            pe.GetVelocity().ToBytes().CopyTo(Data, 12);
            pe.Angles.ToBytes().CopyTo(Data, 12 + 12);
            pe.Gravity.ToBytes().CopyTo(Data, 12 + 12 + 12);
            Utilities.LongToBytes(pe.EID).CopyTo(Data, 12 + 12 + 12 + 12);
        }
    }
}
